# Requirements Document

## Introduction

Tính năng **ML Auto-Retraining Pipeline** tự động hóa quá trình huấn luyện lại các mô hình Machine Learning trong hệ thống Smarthome e-commerce theo lịch hàng tuần. Pipeline bao gồm hai loại mô hình:

1. **Product Recommendation Model** — đề xuất sản phẩm cho người dùng dựa trên lịch sử mua hàng và đánh giá sản phẩm, sử dụng Collaborative Filtering với ML.NET.
2. **Revenue/User Forecasting Model** — dự báo doanh thu và số người dùng mới theo tuần cho admin, sử dụng Time Series Forecasting với ML.NET.

Pipeline chạy dưới dạng .NET `BackgroundService` được tích hợp vào Web project (Blazor Server), không phụ thuộc vào thư viện job scheduler bên ngoài. Admin có thể theo dõi trạng thái và kết quả training qua giao diện Blazor.

---

## Glossary

- **Pipeline**: Quy trình tự động bao gồm các bước thu thập dữ liệu, huấn luyện mô hình, đánh giá và lưu trữ kết quả.
- **Retraining_Scheduler**: Background service chịu trách nhiệm kích hoạt pipeline theo lịch hàng tuần.
- **Recommendation_Trainer**: Thành phần huấn luyện mô hình Product Recommendation sử dụng Collaborative Filtering.
- **Forecasting_Trainer**: Thành phần huấn luyện mô hình Revenue/User Forecasting sử dụng Time Series.
- **Model_Store**: Thành phần lưu trữ và quản lý các file mô hình đã được huấn luyện (`.zip` format của ML.NET).
- **Training_Log**: Bản ghi kết quả của mỗi lần chạy pipeline, bao gồm thời gian, trạng thái, metrics và thông báo lỗi.
- **Recommendation_Engine**: Thành phần sử dụng mô hình đã huấn luyện để tạo danh sách sản phẩm đề xuất cho người dùng.
- **Forecasting_Engine**: Thành phần sử dụng mô hình đã huấn luyện để tạo dự báo doanh thu và người dùng mới.
- **Admin_Dashboard**: Giao diện Blazor dành cho admin để theo dõi trạng thái pipeline và xem kết quả dự báo.
- **Training_Run**: Một lần thực thi đầy đủ của pipeline (bao gồm cả hai mô hình).
- **RMSE**: Root Mean Square Error — chỉ số đánh giá độ chính xác của mô hình dự báo.
- **MAE**: Mean Absolute Error — chỉ số đánh giá độ chính xác của mô hình đề xuất.
- **Minimum_Training_Records**: Số lượng bản ghi tối thiểu cần có để pipeline có thể huấn luyện mô hình (mặc định: 50 bản ghi cho recommendation, 12 tuần dữ liệu cho forecasting).

---

## Requirements

### Requirement 1: Lập lịch tự động hàng tuần

**User Story:** As an admin, I want the ML pipeline to retrain automatically every week, so that the models stay up-to-date with the latest purchase and sales data without manual intervention.

#### Acceptance Criteria

1. THE Retraining_Scheduler SHALL execute the full Training_Run once every 7 days.
2. WHEN the application starts, THE Retraining_Scheduler SHALL calculate the time remaining until the next scheduled Training_Run based on the last recorded execution time stored in the database.
3. IF a Training_Run is already in progress, THEN THE Retraining_Scheduler SHALL skip the scheduled trigger and log a warning indicating the pipeline is busy.
4. WHEN a Training_Run completes (successfully or with failure), THE Retraining_Scheduler SHALL record the completion timestamp to the Training_Log for use in next-run scheduling.
5. THE Retraining_Scheduler SHALL allow admin to trigger a manual Training_Run on demand outside of the weekly schedule.
6. IF the application restarts during a Training_Run, THEN THE Retraining_Scheduler SHALL mark the interrupted Training_Run as failed in the Training_Log and schedule the next run normally.

---

### Requirement 2: Thu thập và chuẩn bị dữ liệu huấn luyện

**User Story:** As a developer, I want the pipeline to extract training data from existing database entities, so that models are trained on real business data.

#### Acceptance Criteria

1. WHEN preparing data for the Recommendation_Trainer, THE Pipeline SHALL query `OrderItem` records joined with `Order` (status = Completed), `ProductRating`, and `ApplicationUser` from the database using EF Core.
2. WHEN preparing data for the Forecasting_Trainer, THE Pipeline SHALL aggregate completed `Order` records by ISO week number to produce weekly revenue totals and count new `ApplicationUser` registrations per week.
3. IF the number of available training records for the Recommendation_Trainer is fewer than Minimum_Training_Records (50), THEN THE Pipeline SHALL skip Recommendation_Trainer training, record a skipped status in the Training_Log, and continue to the Forecasting_Trainer.
4. IF the number of available weekly data points for the Forecasting_Trainer is fewer than Minimum_Training_Records (12 weeks), THEN THE Pipeline SHALL skip Forecasting_Trainer training, record a skipped status in the Training_Log, and complete the Training_Run.
5. THE Pipeline SHALL use only data from the past 2 years when building training datasets to limit memory usage and keep models relevant.

---

### Requirement 3: Huấn luyện mô hình Product Recommendation

**User Story:** As a customer, I want to receive product recommendations based on what similar users have purchased, so that I can discover relevant products more easily.

#### Acceptance Criteria

1. WHEN sufficient training data is available, THE Recommendation_Trainer SHALL train a Collaborative Filtering model using `Microsoft.ML` Matrix Factorization algorithm with `UserId` and `ProductId` as key columns and `Rating` (derived from purchase count or explicit ProductRating) as the label column.
2. WHEN training completes, THE Recommendation_Trainer SHALL evaluate the model using a held-out validation set (20% of training data) and record the MAE metric in the Training_Log.
3. IF the trained model's MAE is lower than the previously saved model's MAE, THEN THE Model_Store SHALL replace the active recommendation model file with the newly trained model.
4. IF the trained model's MAE is equal to or higher than the previously saved model's MAE, THEN THE Model_Store SHALL retain the existing active model and record the comparison result in the Training_Log.
5. IF no previously saved model exists, THEN THE Model_Store SHALL save the newly trained model as the active model regardless of MAE value.
6. THE Recommendation_Trainer SHALL save the trained model as a `.zip` file to a configurable file system path defined in application settings.

---

### Requirement 4: Huấn luyện mô hình Revenue/User Forecasting

**User Story:** As an admin, I want weekly revenue and new user forecasts, so that I can plan inventory and marketing campaigns in advance.

#### Acceptance Criteria

1. WHEN sufficient weekly data is available, THE Forecasting_Trainer SHALL train two separate Time Series Forecasting models using `Microsoft.ML.TimeSeries` Singular Spectrum Analysis (SSA): one for weekly revenue and one for weekly new user count.
2. WHEN training completes, THE Forecasting_Trainer SHALL generate a forecast for the next 4 weeks and store the predicted values in the Training_Log.
3. WHEN training completes, THE Forecasting_Trainer SHALL evaluate each model using the last 4 weeks of historical data as a test set and record the RMSE metric in the Training_Log.
4. IF the trained revenue forecasting model's RMSE is lower than the previously saved model's RMSE, THEN THE Model_Store SHALL replace the active revenue forecasting model file.
5. IF the trained user forecasting model's RMSE is lower than the previously saved model's RMSE, THEN THE Model_Store SHALL replace the active user forecasting model file.
6. IF no previously saved forecasting model exists, THEN THE Model_Store SHALL save the newly trained model as the active model regardless of RMSE value.
7. THE Forecasting_Trainer SHALL save each trained model as a `.zip` file to a configurable file system path defined in application settings.

---

### Requirement 5: Ghi nhật ký và theo dõi trạng thái pipeline

**User Story:** As an admin, I want to see the history and status of each training run, so that I can diagnose failures and monitor model quality over time.

#### Acceptance Criteria

1. THE Pipeline SHALL create a Training_Log entry at the start of each Training_Run, recording the start timestamp and a status of `Running`.
2. WHEN a Training_Run completes successfully, THE Pipeline SHALL update the Training_Log entry with status `Completed`, end timestamp, and metric values (MAE for recommendation, RMSE for each forecasting model).
3. WHEN a Training_Run fails due to an unhandled exception, THE Pipeline SHALL update the Training_Log entry with status `Failed`, end timestamp, and the exception message (truncated to 2000 characters).
4. WHEN a model training step is skipped due to insufficient data, THE Pipeline SHALL record status `Skipped` for that step in the Training_Log entry along with the reason.
5. THE Training_Log SHALL persist Training_Run records to the SQL Server database via EF Core so that history survives application restarts.
6. THE Pipeline SHALL retain the last 52 Training_Log entries in the database and delete older entries automatically after each successful Training_Run.

---

### Requirement 6: API đề xuất sản phẩm cho người dùng

**User Story:** As a customer, I want to see a list of recommended products on the homepage and product detail pages, so that I can find products that match my interests.

#### Acceptance Criteria

1. WHEN a logged-in user requests product recommendations, THE Recommendation_Engine SHALL load the active recommendation model from the Model_Store and return the top 10 products with the highest predicted rating for that user.
2. WHEN the active recommendation model file does not exist, THE Recommendation_Engine SHALL return an empty list and log a warning.
3. WHEN a user has no purchase history, THE Recommendation_Engine SHALL return an empty list.
4. THE Recommendation_Engine SHALL filter out products that are inactive (`IsActive = false`) from the recommendation results before returning them to the caller.
5. WHEN the Recommendation_Engine loads a model, THE Recommendation_Engine SHALL cache the loaded `PredictionEngine` instance for the duration of the application lifetime and reload it only when the model file is updated by the Model_Store.

---

### Requirement 7: API dự báo doanh thu và người dùng cho admin

**User Story:** As an admin, I want to view revenue and new user forecasts for the next 4 weeks on the dashboard, so that I can make data-driven business decisions.

#### Acceptance Criteria

1. WHEN an admin requests the forecast data, THE Forecasting_Engine SHALL load the active revenue and user forecasting models from the Model_Store and return predicted values for the next 4 weeks.
2. WHEN an active forecasting model file does not exist, THE Forecasting_Engine SHALL return an empty forecast result and log a warning.
3. THE Forecasting_Engine SHALL include the forecast week label (e.g., "Tuần 1", "Tuần 2") alongside each predicted value in the response.
4. THE Forecasting_Engine SHALL include the lower and upper confidence interval bounds provided by the SSA model alongside each predicted value.
5. WHEN the Forecasting_Engine loads a model, THE Forecasting_Engine SHALL cache the loaded model instance and reload it only when the model file is updated by the Model_Store.

---

### Requirement 8: Giao diện Admin Dashboard

**User Story:** As an admin, I want a dedicated page in the admin panel to monitor the ML pipeline and view forecasts, so that I have full visibility into the system's ML capabilities.

#### Acceptance Criteria

1. THE Admin_Dashboard SHALL display the status, start time, end time, and metrics of the last 10 Training_Run entries retrieved from the Training_Log.
2. THE Admin_Dashboard SHALL display a button to trigger a manual Training_Run, which is disabled while a Training_Run is in progress.
3. WHEN a manual Training_Run is triggered from the Admin_Dashboard, THE Admin_Dashboard SHALL display a loading indicator and update the Training_Log table in real time using Blazor component state refresh.
4. THE Admin_Dashboard SHALL display the 4-week revenue forecast as a line chart using MudBlazor chart components.
5. THE Admin_Dashboard SHALL display the 4-week new user forecast as a line chart using MudBlazor chart components.
6. WHEN no forecast data is available (model not yet trained), THE Admin_Dashboard SHALL display a descriptive message indicating that the model has not been trained yet.
7. THE Admin_Dashboard SHALL be accessible only to users with the `Admin` role.

---

### Requirement 9: Cấu hình pipeline

**User Story:** As a developer, I want all pipeline parameters to be configurable via application settings, so that I can adjust behavior without recompiling the application.

#### Acceptance Criteria

1. THE Pipeline SHALL read the model storage directory path from the `appsettings.json` key `MlPipeline:ModelStorePath`.
2. THE Pipeline SHALL read the weekly retraining interval (in days) from the `appsettings.json` key `MlPipeline:RetrainingIntervalDays`, defaulting to 7 if the key is absent.
3. THE Pipeline SHALL read the Minimum_Training_Records thresholds from `appsettings.json` keys `MlPipeline:MinRecommendationRecords` (default: 50) and `MlPipeline:MinForecastingWeeks` (default: 12).
4. IF the `MlPipeline:ModelStorePath` directory does not exist at application startup, THEN THE Pipeline SHALL create the directory automatically.
