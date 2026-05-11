using System.IO;
using GestionComercial.Application.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GestionComercial.Presentation.Services;

public class TicketPdfService
{
    public string GenerarTicket(VentaDTO venta)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var carpeta = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "GestionComercial", "Tickets");
        Directory.CreateDirectory(carpeta);

        var archivo = $"Ticket_{Sanitizar(venta.NumeroVenta)}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
        var ruta = Path.Combine(carpeta, archivo);

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5);
                page.MarginHorizontal(1.5f, Unit.Centimetre);
                page.MarginVertical(1.2f, Unit.Centimetre);
                page.DefaultTextStyle(t => t.FontFamily("Arial").FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().AlignCenter()
                        .Text("NOTA DE VENTA / ORDEN DE DESPACHO")
                        .Bold().FontSize(13).FontColor(Colors.Blue.Darken3);
                    col.Item().Height(4);
                    col.Item().LineHorizontal(1.5f).LineColor(Colors.Blue.Darken3);
                    col.Item().Height(4);
                });

                page.Content().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text(t =>
                        {
                            t.Span("N° Orden: ").SemiBold();
                            t.Span(venta.NumeroVenta);
                        });
                        row.RelativeItem().AlignRight().Text(t =>
                        {
                            t.Span("Fecha: ").SemiBold();
                            t.Span(venta.FechaVenta.ToString("dd/MM/yyyy HH:mm"));
                        });
                    });

                    col.Item().Height(4);
                    col.Item().Text(t =>
                    {
                        t.Span("Cliente: ").SemiBold();
                        t.Span(venta.ClienteNombre);
                    });

                    if (!string.IsNullOrWhiteSpace(venta.VehiculoPlaca))
                    {
                        col.Item().Height(2);
                        col.Item().Text(t =>
                        {
                            t.Span("Vehículo (Placa): ").SemiBold();
                            t.Span(venta.VehiculoPlaca);
                        });
                    }

                    col.Item().Height(2);
                    col.Item().Text(t =>
                    {
                        t.Span("Tipo Documento: ").SemiBold();
                        t.Span(venta.TipoDocumento);
                    });

                    col.Item().Height(10);
                    col.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);
                    col.Item().Height(6);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(4);
                            cols.RelativeColumn(2);
                            cols.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell()
                                .Background(Colors.Blue.Darken3).Padding(4)
                                .DefaultTextStyle(t => t.FontColor(Colors.White).SemiBold().FontSize(9))
                                .Text("Descripción");
                            header.Cell()
                                .Background(Colors.Blue.Darken3).Padding(4)
                                .DefaultTextStyle(t => t.FontColor(Colors.White).SemiBold().FontSize(9))
                                .AlignRight().Text("Cantidad (kg)");
                            header.Cell()
                                .Background(Colors.Blue.Darken3).Padding(4)
                                .DefaultTextStyle(t => t.FontColor(Colors.White).SemiBold().FontSize(9))
                                .AlignRight().Text("Importe ($)");
                        });

                        table.Cell().Padding(5).Text("Material / Producto");
                        table.Cell().Padding(5).AlignRight().Text($"{venta.PesoNetoKg:N2}");
                        table.Cell().Padding(5).AlignRight().Text($"{venta.Subtotal:N2}");
                    });

                    col.Item().Height(8);

                    if (venta.DescuentosAplicados > 0)
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem();
                            row.ConstantItem(110).AlignRight().Text("Descuento:").FontSize(10);
                            row.ConstantItem(70).AlignRight().Text($"-${venta.DescuentosAplicados:N2}").FontSize(10);
                        });
                        col.Item().Height(3);
                    }

                    if (venta.IVA > 0)
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem();
                            row.ConstantItem(110).AlignRight().Text("IVA:").FontSize(10);
                            row.ConstantItem(70).AlignRight().Text($"${venta.IVA:N2}").FontSize(10);
                        });
                        col.Item().Height(3);
                    }

                    col.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);
                    col.Item().Height(3);
                    col.Item().Row(row =>
                    {
                        row.RelativeItem();
                        row.ConstantItem(110).AlignRight().Text("TOTAL:").Bold().FontSize(12);
                        row.ConstantItem(70).AlignRight().Text($"${venta.TotalVenta:N2}").Bold().FontSize(12).FontColor(Colors.Blue.Darken3);
                    });

                    col.Item().Height(12);
                    col.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);
                    col.Item().Height(4);

                    col.Item().Row(row =>
                    {
                        if (venta.PesoTaraKg > 0)
                            row.RelativeItem().Text(t =>
                            {
                                t.Span("Tara: ").SemiBold().FontSize(9);
                                t.Span($"{venta.PesoTaraKg:N2} kg").FontSize(9);
                            });

                        if (venta.PesoBrutoKg > 0)
                            row.RelativeItem().Text(t =>
                            {
                                t.Span("Bruto: ").SemiBold().FontSize(9);
                                t.Span($"{venta.PesoBrutoKg:N2} kg").FontSize(9);
                            });

                        row.RelativeItem().Text(t =>
                        {
                            t.Span("Neto: ").SemiBold().FontSize(9);
                            t.Span($"{venta.PesoNetoKg:N2} kg").FontSize(9);
                        });
                    });

                    col.Item().Height(28);
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(inner =>
                        {
                            inner.Item().AlignCenter().Text("________________________");
                            inner.Item().AlignCenter().Text("Firma Receptor").FontSize(9).FontColor(Colors.Grey.Darken1);
                        });
                        row.ConstantItem(20);
                        row.RelativeItem().Column(inner =>
                        {
                            inner.Item().AlignCenter().Text("________________________");
                            inner.Item().AlignCenter().Text("Autorización / Sello").FontSize(9).FontColor(Colors.Grey.Darken1);
                        });
                    });

                    col.Item().Height(10);
                    col.Item().AlignCenter()
                        .Text("Documento válido solo para despacho de material. No constituye factura.")
                        .FontSize(8).Italic().FontColor(Colors.Grey.Darken1);
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Generado: ").FontSize(8).FontColor(Colors.Grey.Darken1);
                    t.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")).FontSize(8).FontColor(Colors.Grey.Darken1);
                    t.Span("   |   COMPLETADA").FontSize(8).FontColor(Colors.Green.Darken2);
                });
            });
        }).GeneratePdf(ruta);

        return ruta;
    }

    private static string Sanitizar(string nombre)
        => string.Concat(nombre.Split(Path.GetInvalidFileNameChars()));
}
