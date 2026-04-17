namespace Infrastructure.Services.ML;

using Application.DTOs.ML;
using Application.Interfaces;

/// <summary>
/// Service sử dụng thuật toán Collaborative Filtering đơn giản để gợi ý sản phẩm
/// Lưu ý: Để sử dụng ML.NET Model Builder đầy đủ, bạn cần cài đặt Visual Studio extension
/// </summary>
public class UserSimilarityService : IUserSimilarityService
{
    private Dictionary<(uint UserId, uint ProductId), float> _ratings = new();
    private Dictionary<uint, List<(uint ProductId, float Rating)>> _userRatings = new();
    private Dictionary<uint, List<(uint UserId, float Rating)>> _productRatings = new();
    private bool _isTrained = false;

    public UserSimilarityService(string modelPath = "models/user_similarity_model.zip")
    {
        // Model path được lưu nhưng chưa sử dụng trong implementation đơn giản này
    }

    /// <summary>
    /// Train model từ dữ liệu rating của người dùng
    /// </summary>
    public void Train(IEnumerable<UserProductRatingDto> trainingData)
    {
        // Xây dựng cấu trúc dữ liệu cho collaborative filtering
        _ratings.Clear();
        _userRatings.Clear();
        _productRatings.Clear();

        foreach (var rating in trainingData)
        {
            _ratings[(rating.UserId, rating.ProductId)] = rating.Rating;

            if (!_userRatings.ContainsKey(rating.UserId))
                _userRatings[rating.UserId] = new List<(uint, float)>();
            _userRatings[rating.UserId].Add((rating.ProductId, rating.Rating));

            if (!_productRatings.ContainsKey(rating.ProductId))
                _productRatings[rating.ProductId] = new List<(uint, float)>();
            _productRatings[rating.ProductId].Add((rating.UserId, rating.Rating));
        }

        _isTrained = true;
    }

    /// <summary>
    /// Load model đã train từ file
    /// </summary>
    public void LoadModel()
    {
        // Trong implementation đơn giản này, không load từ file
        // Có thể implement sau nếu cần lưu model
        _isTrained = false;
    }

    /// <summary>
    /// Predict rating cho một người dùng và sản phẩm cụ thể
    /// </summary>
    public float PredictRating(uint userId, uint productId)
    {
        if (!_isTrained)
            throw new InvalidOperationException("Model chưa được train. Gọi Train() trước.");

        // Nếu đã có rating, trả về rating đó
        if (_ratings.ContainsKey((userId, productId)))
            return _ratings[(userId, productId)];

        // Sử dụng User-based Collaborative Filtering
        return PredictRatingUserBased(userId, productId);
    }

    /// <summary>
    /// Lấy danh sách sản phẩm được gợi ý cho người dùng
    /// </summary>
    public IEnumerable<(uint ProductId, float Score)> RecommendProducts(uint userId, IEnumerable<uint> productIds, int topN = 10)
    {
        if (!_isTrained)
            throw new InvalidOperationException("Model chưa được train. Gọi Train() trước.");

        var predictions = productIds.Select(productId =>
        {
            var score = PredictRating(userId, productId);
            return (ProductId: productId, Score: score);
        });

        return predictions.OrderByDescending(p => p.Score).Take(topN);
    }

    /// <summary>
    /// Tìm người dùng tương tự dựa trên cosine similarity
    /// </summary>
    public IEnumerable<(uint UserId, float SimilarityScore)> FindSimilarUsers(uint targetUserId, IEnumerable<uint> candidateUserIds, int topN = 10)
    {
        if (!_isTrained)
            throw new InvalidOperationException("Model chưa được train. Gọi Train() trước.");

        if (!_userRatings.ContainsKey(targetUserId))
            return Enumerable.Empty<(uint, float)>();

        var similarities = candidateUserIds.Select(candidateUserId =>
        {
            var similarity = CalculateCosineSimilarity(targetUserId, candidateUserId);
            return (candidateUserId, similarity);
        });

        return similarities.OrderByDescending(s => s.Item2).Take(topN);
    }

    private float PredictRatingUserBased(uint userId, uint productId)
    {
        // Tìm các người dùng tương tự
        var similarUsers = _productRatings.ContainsKey(productId)
            ? _productRatings[productId].Select(r => r.UserId).ToList()
            : new List<uint>();

        if (!similarUsers.Any())
            return 0f;

        // Tính toán weighted average dựa trên similarity
        float weightedSum = 0;
        float similaritySum = 0;

        foreach (var similarUserId in similarUsers)
        {
            var similarity = CalculateCosineSimilarity(userId, similarUserId);
            var rating = _ratings[(similarUserId, productId)];

            weightedSum += similarity * rating;
            similaritySum += Math.Abs(similarity);
        }

        return similaritySum > 0 ? weightedSum / similaritySum : 0f;
    }

    private float CalculateCosineSimilarity(uint user1, uint user2)
    {
        if (!_userRatings.ContainsKey(user1) || !_userRatings.ContainsKey(user2))
            return 0f;

        var ratings1 = _userRatings[user1];
        var ratings2 = _userRatings[user2];

        // Tìm các sản phẩm chung
        var commonProducts = ratings1.Select(r => r.ProductId)
            .Intersect(ratings2.Select(r => r.ProductId))
            .ToList();

        if (!commonProducts.Any())
            return 0f;

        // Tính cosine similarity
        float dotProduct = 0;
        float magnitude1 = 0;
        float magnitude2 = 0;

        foreach (var productId in commonProducts)
        {
            var r1 = ratings1.First(r => r.ProductId == productId).Rating;
            var r2 = ratings2.First(r => r.ProductId == productId).Rating;

            dotProduct += r1 * r2;
            magnitude1 += r1 * r1;
            magnitude2 += r2 * r2;
        }

        if (magnitude1 == 0 || magnitude2 == 0)
            return 0f;

        return dotProduct / (float)(Math.Sqrt(magnitude1) * Math.Sqrt(magnitude2));
    }
}
