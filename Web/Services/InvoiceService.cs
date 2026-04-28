using Application.DTOs.Responses;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Web.Services
{
    public interface IInvoiceService
    {
        byte[] GenerateInvoicePdf(OrderResponse order);
    }

    public class InvoiceService : IInvoiceService
    {
        private readonly ILogger<InvoiceService> _logger;

        public InvoiceService(ILogger<InvoiceService> logger)
        {
            _logger = logger;
        }

        public byte[] GenerateInvoicePdf(OrderResponse order)
        {
            try
            {
                _logger.LogInformation("Starting PDF generation for order {OrderNumber}", order?.OrderNumber);
                
                if (order == null)
                {
                    _logger.LogError("Order is null");
                    throw new ArgumentNullException(nameof(order));
                }

                if (order.Items == null || !order.Items.Any())
                {
                    _logger.LogWarning("Order {OrderNumber} has no items", order.OrderNumber);
                }

                var document = new InvoiceDocument(order);
                var pdfBytes = document.GeneratePdf();
                
                _logger.LogInformation("Successfully generated PDF for order {OrderNumber}, size: {Size} bytes", order.OrderNumber, pdfBytes.Length);
                return pdfBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF for order {OrderNumber}", order?.OrderNumber ?? "unknown");
                throw;
            }
        }
    }

    public class InvoiceDocument : IDocument
    {
        private readonly OrderResponse _order;

        public InvoiceDocument(OrderResponse order)
        {
            _order = order;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    // 1. Xác định khổ giấy rõ ràng (A4)
                    page.Size(PageSizes.A4);
                    page.Margin(50);
                    page.DefaultTextStyle(x => x.FontSize(9).LineHeight(1.3f).FontFamily(Fonts.Verdana)); // Đảm bảo font hỗ trợ Unicode nếu cần

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);
                    
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Trang ");
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
        }

        void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("HÓA ĐƠN").Bold().FontSize(20).FontColor(Colors.Blue.Medium);
                    column.Item().Text($"Mã đơn hàng: #{_order.OrderNumber}");
                    column.Item().Text($"Ngày tạo: {_order.CreatedAt:dd/MM/yyyy HH:mm}");
                });

                row.RelativeItem().AlignRight().Column(column =>
                {
                    column.Item().Text("Đại học CT").Bold().FontSize(14);
                    column.Item().Text("SĐT: 0397765046");
                });
            });
        }

        void ComposeContent(IContainer container)
        {
            container.PaddingVertical(40).Column(column =>
            {
                column.Item().Element(ComposeCustomerInfo);
                column.Item().PaddingVertical(10);
                column.Item().Element(ComposeOrderItems);
                column.Item().PaddingVertical(10);
                column.Item().Element(ComposeTotals);
            });
        }

        void ComposeCustomerInfo(IContainer container)
        {
            container.Border(1).Padding(10).Column(column =>
            {
                column.Item().Text("Thông tin khách hàng").Bold().FontSize(11);
                column.Item().PaddingTop(8).Text($"Tên: {_order.ReceiverName}").FontSize(9);
                column.Item().PaddingTop(4).Text($"SĐT: {_order.ReceiverPhone}").FontSize(9);
                column.Item().PaddingTop(4).Text($"Địa chỉ: {_order.ShippingAddress}").FontSize(9);
                column.Item().PaddingTop(4).Text($"Phương thức thanh toán: {_order.PaymentMethod}").FontSize(9);
                column.Item().PaddingTop(4).Text($"Trạng thái: {_order.Status}").FontSize(9);
            });
        }

        void ComposeOrderItems(IContainer container)
        {
            container.Table(table =>
            {
                // 2. Điều chỉnh lại tỷ lệ cột để phần tên sản phẩm (Relative) chiếm không gian chính giữa
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(30);  // STT
                    columns.RelativeColumn();    // Sản phẩm (tự động giãn)
                    columns.ConstantColumn(80);  // Đơn giá
                    columns.ConstantColumn(40);  // SL
                    columns.ConstantColumn(100); // Thành tiền
                });

                table.Header(header =>
                {
                    header.Cell().Element(HeaderStyle).Text("STT");
                    header.Cell().Element(HeaderStyle).Text("Sản phẩm");
                    header.Cell().Element(HeaderStyle).AlignRight().Text("Đơn giá");
                    header.Cell().Element(HeaderStyle).AlignRight().Text("SL");
                    header.Cell().Element(HeaderStyle).AlignRight().Text("Thành tiền");
                });

                int index = 1;
                foreach (var item in _order.Items)
                {
                    table.Cell().Element(CellStyle).Text($"{index++}");
                    table.Cell().Element(CellStyle).Text(item.ProductName);
                    table.Cell().Element(CellStyle).AlignRight().Text($"{item.UnitPrice:N0}");
                    table.Cell().Element(CellStyle).AlignRight().Text(item.Quantity.ToString());
                    table.Cell().Element(CellStyle).AlignRight().Text($"{item.TotalPrice:N0}");
                }
            });
        }

        void ComposeTotals(IContainer container)
        {
            // 3. Sử dụng Row với RelativeItem trống bên trái để đẩy nội dung sang phải một cách cân đối
            container.PaddingTop(10).Row(row => 
            {
                row.RelativeItem(); // Chiếm hết phần trống bên trái
                
                row.ConstantItem(250).Column(column => // Giới hạn độ rộng vùng tổng tiền
                {
                    column.Item().Row(r => {
                        r.RelativeItem().Text("Tạm tính:");
                        r.ConstantItem(100).AlignRight().Text($"{_order.SubTotal:N0} đ");
                    });
                    column.Item().Row(r => {
                        r.RelativeItem().Text("Phí vận chuyển:");
                        r.ConstantItem(100).AlignRight().Text($"{_order.ShippingFee:N0} đ");
                    });
                    column.Item().Row(r => {
                        r.RelativeItem().Text("Giảm giá:");
                        r.ConstantItem(100).AlignRight().Text($"- {_order.DiscountAmount:N0} đ");
                    });
                    column.Item().PaddingTop(5).BorderTop(1).Row(r => {
                        r.RelativeItem().Text("Tổng cộng:").Bold().FontSize(12);
                        r.ConstantItem(100).AlignRight().Text($"{_order.TotalAmount:N0} đ").Bold().FontSize(12);
                    });
                });
            });
        }

        // 4. Sửa lại Style để có đầy đủ viền, tránh cảm giác lệch lạc
        static IContainer HeaderStyle(IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(Colors.Black)
                .PaddingVertical(5)
                .PaddingHorizontal(5)
                .DefaultTextStyle(x => x.Bold());
        }

        static IContainer CellStyle(IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .PaddingVertical(5)
                .PaddingHorizontal(5);
        }
    }
}
