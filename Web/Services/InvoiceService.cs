using Application.DTOs.Responses;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Web.Services
{
    public interface IInvoiceService
    {
        byte[] GenerateInvoicePdf(OrderResponse order);
    }

    public class InvoiceService : IInvoiceService
    {
        public byte[] GenerateInvoicePdf(OrderResponse order)
        {
            var document = new InvoiceDocument(order);
            return document.GeneratePdf();
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
                    page.Margin(50);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                    });
                });
        }

        void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("HÓA ĐƠN").Bold().FontSize(20);
                    column.Item().Text($"Mã đơn hàng: #{_order.OrderNumber}").FontSize(12);
                    column.Item().Text($"Ngày tạo: {_order.CreatedAt:dd/MM/yyyy HH:mm}").FontSize(10);
                });

                row.RelativeItem().Column(column =>
                {
                    column.Item().AlignRight().Text("SmartHome").Bold().FontSize(16);
                    column.Item().AlignRight().Text("Địa chỉ: 123 Đường ABC, TP.HCM").FontSize(10);
                    column.Item().AlignRight().Text("Điện thoại: 0901234567").FontSize(10);
                    column.Item().AlignRight().Text("Email: contact@smarthome.vn").FontSize(10);
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
                column.Item().Text("Thông tin khách hàng").Bold().FontSize(12);
                column.Item().PaddingTop(5).Text($"Tên: {_order.ReceiverName}").FontSize(10);
                column.Item().Text($"SĐT: {_order.ReceiverPhone}").FontSize(10);
                column.Item().Text($"Địa chỉ: {_order.ShippingAddress}").FontSize(10);
                column.Item().Text($"Phương thức thanh toán: {_order.PaymentMethod}").FontSize(10);
                column.Item().Text($"Trạng thái: {_order.Status}").FontSize(10);
            });
        }

        void ComposeOrderItems(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantItem(50);
                    columns.RelativeItem();
                    columns.ConstantItem(80);
                    columns.ConstantItem(80);
                    columns.ConstantItem(100);
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("STT").Bold();
                    header.Cell().Element(CellStyle).Text("Sản phẩm").Bold();
                    header.Cell().Element(CellStyle).AlignRight().Text("Đơn giá").Bold();
                    header.Cell().Element(CellStyle).AlignRight().Text("SL").Bold();
                    header.Cell().Element(CellStyle).AlignRight().Text("Thành tiền").Bold();
                });

                int index = 1;
                foreach (var item in _order.Items)
                {
                    table.Cell().Element(CellStyle).Text($"{index++}");
                    table.Cell().Element(CellStyle).Text(item.ProductName);
                    table.Cell().Element(CellStyle).AlignRight().Text($"{item.UnitPrice:N0} đ");
                    table.Cell().Element(CellStyle).AlignRight().Text(item.Quantity.ToString());
                    table.Cell().Element(CellStyle).AlignRight().Text($"{item.TotalPrice:N0} đ");
                }
            });
        }

        void ComposeTotals(IContainer container)
        {
            container.AlignRight().Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().AlignRight().Text("Tạm tính:");
                    row.ConstantColumn(150).AlignRight().Text($"{_order.SubTotal:N0} đ");
                });

                column.Item().Row(row =>
                {
                    row.RelativeItem().AlignRight().Text("Phí vận chuyển:");
                    row.ConstantColumn(150).AlignRight().Text($"{_order.ShippingFee:N0} đ");
                });

                column.Item().Row(row =>
                {
                    row.RelativeItem().AlignRight().Text("Giảm giá:");
                    row.ConstantColumn(150).AlignRight().Text($"- {_order.DiscountAmount:N0} đ");
                });

                column.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().AlignRight().Text("Tổng cộng:").Bold().FontSize(12);
                    row.ConstantColumn(150).AlignRight().Text($"{_order.TotalAmount:N0} đ").Bold().FontSize(12);
                });
            });
        }

        static IContainer CellStyle(IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderLeft(1)
                .BorderRight(1)
                .PaddingVertical(5)
                .PaddingHorizontal(10);
        }
    }
}
