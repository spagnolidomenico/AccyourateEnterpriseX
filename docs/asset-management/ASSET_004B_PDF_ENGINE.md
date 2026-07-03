# ASSET-004B - PDF Engine

Introduce un motore PDF riutilizzabile e `DeliveryReportPdfService`.

Output PDF:

```text
Documenti/Accyourate Enterprise X/Verbali Consegna
```

Componenti:
- `SimplePdfDocument`
- `SimplePdfWriter`
- `PdfExportService`
- `DeliveryReportPdfService`
- `DeliveryReportRepository.GetById`
- `DeliveryReportRepository.UpdatePdfPath`
