# Responsive UI Audit

Generato: 2026-09-01 16:48:57

- File C# analizzati: 456
- File con segnalazioni: 74
- High: 174
- Medium: 43
- Low: 42

## Priorita High

| File | Riga | Regola | Dettaglio | Correzione consigliata |
|---|---:|---|---|---|
| src\Accyourate.App\ActionEngineWindow.cs | 16 | LargeMinWidth | MinWidth impostata a 1040 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AiIntentCatalogManagerWindow.cs | 27 | LargeMinWidth | MinWidth impostata a 1040 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AnalyticsDashboardWindow.cs | 29 | LargeMinWidth | MinWidth impostata a 1180 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AppleStyleDashboardWindow.cs | 34 | LargeMinWidth | MinWidth impostata a 1180 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\ArchitectureWindow.cs | 26 | LargeMinWidth | MinWidth impostata a 1024 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\ArchitectureWindow.cs | 58 | HorizontalAutoScroll | Scorrimento orizzontale automatico presente. | Verificare che sia limitato a una tabella reale e non all'intera pagina. |
| src\Accyourate.App\AssetManagement\Deliveries\DeliveryRegisterView.cs | 97 | LargeMinWidth | MinWidth impostata a 1120 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\Deliveries\DeliveryRegisterView.cs | 105 | HorizontalAutoScroll | Scorrimento orizzontale automatico presente. | Verificare che sia limitato a una tabella reale e non all'intera pagina. |
| src\Accyourate.App\AssetManagement\Deliveries\DeliveryRegisterView.cs | 333 | RigidWideGrid | Griglia con 8 colonne fisse e almeno 1010 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\AssetManagement\LocationStocktakeView.cs | 18 | HorizontalAutoScroll | Scorrimento orizzontale automatico presente. | Verificare che sia limitato a una tabella reale e non all'intera pagina. |
| src\Accyourate.App\AssetManagement\LocationStocktakeView.cs | 19 | LargeMinWidth | MinWidth impostata a 980 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\LocationStocktakeView.cs | 25 | RigidWideGrid | Griglia con 8 colonne fisse e almeno 1075 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\AssetManagement\MaintenanceOperationsView.cs | 173 | LargeMinWidth | MinWidth impostata a 1190 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\MaintenanceOperationsView.cs | 180 | HorizontalAutoScroll | Scorrimento orizzontale automatico presente. | Verificare che sia limitato a una tabella reale e non all'intera pagina. |
| src\Accyourate.App\AssetManagement\MaintenanceOperationsView.cs | 888 | RigidWideGrid | Griglia con 12 colonne fisse e almeno 1375 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\AssetManagement\MaintenancePurchasingView.cs | 71 | HorizontalAutoScroll | Scorrimento orizzontale automatico presente. | Verificare che sia limitato a una tabella reale e non all'intera pagina. |
| src\Accyourate.App\AssetManagement\MaintenancePurchasingView.cs | 112 | LargeMinWidth | MinWidth impostata a 1120 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\MaintenancePurchasingView.cs | 131 | LargeMinWidth | MinWidth impostata a 900 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\MaintenancePurchasingView.cs | 137 | RigidWideGrid | Griglia con 6 colonne fisse e almeno 930 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\AssetManagement\MaintenancePurchasingView.cs | 275 | RigidWideGrid | Griglia con 9 colonne fisse e almeno 1050 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\AssetManagement\SparePartLocationsView.cs | 31 | HorizontalAutoScroll | Scorrimento orizzontale automatico presente. | Verificare che sia limitato a una tabella reale e non all'intera pagina. |
| src\Accyourate.App\AssetManagement\SparePartLocationsView.cs | 39 | LargeMinWidth | MinWidth impostata a 1000 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\SparePartLocationsView.cs | 65 | RigidWideGrid | Griglia con 8 colonne fisse e almeno 1020 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\AssetManagement\SparePartLocationsView.cs | 104 | LargeMinWidth | MinWidth impostata a 950 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\SparePartLocationsView.cs | 125 | LargeMinWidth | MinWidth impostata a 900 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\SparePartLocationsView.cs | 156 | LargeMinWidth | MinWidth impostata a 1040 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\SparePartLocationsView.cs | 181 | RigidWideGrid | Griglia con 7 colonne fisse e almeno 1115 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\AssetManagement\SparePartPickRequestsWindow.cs | 25 | LargeMinWidth | MinWidth impostata a 980 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\SparePartPickRequestsWindow.cs | 39 | HorizontalAutoScroll | Scorrimento orizzontale automatico presente. | Verificare che sia limitato a una tabella reale e non all'intera pagina. |
| src\Accyourate.App\AssetManagement\SparePartPickRequestsWindow.cs | 51 | LargeMinWidth | MinWidth impostata a 1220 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\SparePartPickRequestsWindow.cs | 94 | RigidWideGrid | Griglia con 8 colonne fisse e almeno 1320 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\AssetManagement\SparePartPickRequestsWindow.cs | 127 | LargeMinWidth | MinWidth impostata a 1080 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\SparePartPickRequestsWindow.cs | 138 | RigidWideGrid | Griglia con 9 colonne fisse e almeno 1105 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\AssetManagement\SparePartQuarantineWindow.cs | 21 | LargeMinWidth | MinWidth impostata a 1020 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\SparePartQuarantineWindow.cs | 37 | HorizontalAutoScroll | Scorrimento orizzontale automatico presente. | Verificare che sia limitato a una tabella reale e non all'intera pagina. |
| src\Accyourate.App\AssetManagement\SparePartQuarantineWindow.cs | 48 | LargeMinWidth | MinWidth impostata a 1270 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\SparePartQuarantineWindow.cs | 68 | RigidWideGrid | Griglia con 8 colonne fisse e almeno 1300 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\AssetManagement\SparePartReplenishmentView.cs | 46 | HorizontalAutoScroll | Scorrimento orizzontale automatico presente. | Verificare che sia limitato a una tabella reale e non all'intera pagina. |
| src\Accyourate.App\AssetManagement\SparePartReplenishmentView.cs | 60 | LargeMinWidth | MinWidth impostata a 1230 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\SparePartReplenishmentView.cs | 158 | RigidWideGrid | Griglia con 8 colonne fisse e almeno 1230 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\AssetManagement\SparePartRmaWindow.cs | 16 | LargeMinWidth | MinWidth impostata a 1080 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\SparePartRmaWindow.cs | 17 | HorizontalAutoScroll | Scorrimento orizzontale automatico presente. | Verificare che sia limitato a una tabella reale e non all'intera pagina. |
| src\Accyourate.App\AssetManagement\SparePartRmaWindow.cs | 18 | LargeMinWidth | MinWidth impostata a 1340 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\SparePartRmaWindow.cs | 37 | RigidWideGrid | Griglia con 10 colonne fisse e almeno 1610 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\AssetManagement\SparePartsInventoryAdvancedDialogs.cs | 111 | LargeMinWidth | MinWidth impostata a 900 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\SparePartsInventoryAdvancedDialogs.cs | 149 | LargeMinWidth | MinWidth impostata a 1080 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\SparePartsInventoryAdvancedDialogs.cs | 162 | HorizontalAutoScroll | Scorrimento orizzontale automatico presente. | Verificare che sia limitato a una tabella reale e non all'intera pagina. |
| src\Accyourate.App\AssetManagement\SparePartsInventoryAdvancedDialogs.cs | 168 | RigidWideGrid | Griglia con 9 colonne fisse e almeno 1155 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\AssetManagement\SparePartsInventoryView.cs | 61 | LargeMinWidth | MinWidth impostata a 1180 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\SparePartsInventoryView.cs | 63 | HorizontalAutoScroll | Scorrimento orizzontale automatico presente. | Verificare che sia limitato a una tabella reale e non all'intera pagina. |
| src\Accyourate.App\AssetManagement\SparePartsInventoryView.cs | 138 | RigidWideGrid | Griglia con 8 colonne fisse e almeno 1450 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\AssetManagement\SparePartStocktakeView.cs | 35 | HorizontalAutoScroll | Scorrimento orizzontale automatico presente. | Verificare che sia limitato a una tabella reale e non all'intera pagina. |
| src\Accyourate.App\AssetManagement\SparePartStocktakeView.cs | 42 | LargeMinWidth | MinWidth impostata a 1080 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\SparePartStocktakeView.cs | 96 | RigidWideGrid | Griglia con 9 colonne fisse e almeno 1190 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\AssetManagement\SparePartStocktakeView.cs | 139 | LargeMinWidth | MinWidth impostata a 940 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\SparePartStocktakeView.cs | 172 | RigidWideGrid | Griglia con 7 colonne fisse e almeno 985 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\AssetManagement\SupplierCommunicationArchiveWindow.cs | 19 | HorizontalAutoScroll | Scorrimento orizzontale automatico presente. | Verificare che sia limitato a una tabella reale e non all'intera pagina. |
| src\Accyourate.App\AssetManagement\SupplierCommunicationArchiveWindow.cs | 25 | LargeMinWidth | MinWidth impostata a 940 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\SupplierCommunicationRegisterWindow.cs | 16 | LargeMinWidth | MinWidth impostata a 1100 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\SupplierCommunicationRegisterWindow.cs | 17 | HorizontalAutoScroll | Scorrimento orizzontale automatico presente. | Verificare che sia limitato a una tabella reale e non all'intera pagina. |
| src\Accyourate.App\AssetManagement\SupplierCommunicationRegisterWindow.cs | 27 | LargeMinWidth | MinWidth impostata a 1500 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\SupplierCommunicationRegisterWindow.cs | 27 | RigidWideGrid | Griglia con 7 colonne fisse e almeno 1295 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\AssetManagement\SupplierFollowUpDashboardWindow.cs | 25 | LargeMinWidth | MinWidth impostata a 1050 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\SupplierFollowUpDashboardWindow.cs | 70 | HorizontalAutoScroll | Scorrimento orizzontale automatico presente. | Verificare che sia limitato a una tabella reale e non all'intera pagina. |
| src\Accyourate.App\AssetManagement\SupplierFollowUpDashboardWindow.cs | 110 | LargeMinWidth | MinWidth impostata a 1320 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\SupplierFollowUpDashboardWindow.cs | 110 | RigidWideGrid | Griglia con 8 colonne fisse e almeno 1310 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\AssetManagement\SupplierRmaAuditDashboardWindow.cs | 24 | LargeMinWidth | MinWidth impostata a 980 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\SupplierRmaAuditScheduleWindow.cs | 15 | LargeMinWidth | MinWidth impostata a 920 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\SupplierRmaAuditScheduleWindow.cs | 16 | HorizontalAutoScroll | Scorrimento orizzontale automatico presente. | Verificare che sia limitato a una tabella reale e non all'intera pagina. |
| src\Accyourate.App\AssetManagement\SupplierRmaAuditScheduleWindow.cs | 17 | LargeMinWidth | MinWidth impostata a 1050 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\SupplierRmaAuditScheduleWindow.cs | 17 | LargeMinWidth | MinWidth impostata a 1050 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\SupplierRmaAuditScheduleWindow.cs | 26 | RigidWideGrid | Griglia con 6 colonne fisse e almeno 1140 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\AssetManagement\SupplierRmaAuditScheduleWindow.cs | 26 | RigidWideGrid | Griglia con 6 colonne fisse e almeno 1040 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\AssetManagement\SupplierRmaCapaAttestationRetentionAuditWindow.cs | 15 | HorizontalAutoScroll | Scorrimento orizzontale automatico presente. | Verificare che sia limitato a una tabella reale e non all'intera pagina. |
| src\Accyourate.App\AssetManagement\SupplierRmaCapaAttestationRetentionAuditWindow.cs | 16 | LargeMinWidth | MinWidth impostata a 1180 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\SupplierRmaCapaAttestationRetentionAuditWindow.cs | 18 | RigidWideGrid | Griglia con 6 colonne fisse e almeno 1240 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\AssetManagement\SupplierRmaCapaDossierRegistryWindow.cs | 25 | RigidWideGrid | Griglia con 12 colonne fisse e almeno 1775 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\AssetManagement\SupplierRmaCapaGovernanceReviewRetentionRegistryWindow.cs | 174 | RigidWideGrid | Griglia con 9 colonne fisse e almeno 1245 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\AssetManagement\SupplierRmaCentralAuditRegisterWindow.cs | 19 | LargeMinWidth | MinWidth impostata a 1040 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\SupplierRmaCentralAuditRegisterWindow.cs | 20 | HorizontalAutoScroll | Scorrimento orizzontale automatico presente. | Verificare che sia limitato a una tabella reale e non all'intera pagina. |
| src\Accyourate.App\AssetManagement\SupplierRmaCentralAuditRegisterWindow.cs | 21 | LargeMinWidth | MinWidth impostata a 1250 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\SupplierRmaCentralAuditRegisterWindow.cs | 28 | RigidWideGrid | Griglia con 6 colonne fisse e almeno 990 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\AssetManagement\SupplierRmaComplianceWindow.cs | 15 | LargeMinWidth | MinWidth impostata a 980 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\SupplierRmaComplianceWindow.cs | 16 | HorizontalAutoScroll | Scorrimento orizzontale automatico presente. | Verificare che sia limitato a una tabella reale e non all'intera pagina. |
| src\Accyourate.App\AssetManagement\SupplierRmaComplianceWindow.cs | 17 | LargeMinWidth | MinWidth impostata a 1160 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\SupplierRmaCorrectiveActionsWindow.cs | 19 | LargeMinWidth | MinWidth impostata a 980 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\SupplierRmaCorrectiveActionsWindow.cs | 20 | HorizontalAutoScroll | Scorrimento orizzontale automatico presente. | Verificare che sia limitato a una tabella reale e non all'intera pagina. |
| src\Accyourate.App\AssetManagement\SupplierRmaCorrectiveActionsWindow.cs | 21 | LargeMinWidth | MinWidth impostata a 1380 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\SupplierRmaCorrectiveActionsWindow.cs | 29 | RigidWideGrid | Griglia con 7 colonne fisse e almeno 1030 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\AssetManagement\SupplierRmaDetailWindow.cs | 15 | LargeMinWidth | MinWidth impostata a 900 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\SupplierRmaPerformanceWindow.cs | 22 | LargeMinWidth | MinWidth impostata a 1050 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\SupplierRmaPerformanceWindow.cs | 34 | HorizontalAutoScroll | Scorrimento orizzontale automatico presente. | Verificare che sia limitato a una tabella reale e non all'intera pagina. |
| src\Accyourate.App\AssetManagement\SupplierRmaPerformanceWindow.cs | 42 | LargeMinWidth | MinWidth impostata a 1240 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\SupplierRmaPerformanceWindow.cs | 56 | RigidWideGrid | Griglia con 12 colonne fisse e almeno 1260 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\AssetManagement\SupplierRmaPortalWindow.cs | 17 | LargeMinWidth | MinWidth impostata a 1040 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\SupplierRmaPortalWindow.cs | 20 | HorizontalAutoScroll | Scorrimento orizzontale automatico presente. | Verificare che sia limitato a una tabella reale e non all'intera pagina. |
| src\Accyourate.App\AssetManagement\SupplierRmaPortalWindow.cs | 20 | LargeMinWidth | MinWidth impostata a 1510 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\SupplierRmaPortalWindow.cs | 20 | RigidWideGrid | Griglia con 7 colonne fisse e almeno 1190 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\AssetManagement\SupplierRmaValidationRegisterWindow.cs | 25 | LargeMinWidth | MinWidth impostata a 920 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetManagement\SupplierRmaValidationRegisterWindow.cs | 42 | HorizontalAutoScroll | Scorrimento orizzontale automatico presente. | Verificare che sia limitato a una tabella reale e non all'intera pagina. |
| src\Accyourate.App\AssetManagement\SupplierRmaValidationRegisterWindow.cs | 51 | LargeMinWidth | MinWidth impostata a 1120 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetsWindow.cs | 43 | LargeMinWidth | MinWidth impostata a 1180 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\AssetsWindow.cs | 63 | HorizontalAutoScroll | Scorrimento orizzontale automatico presente. | Verificare che sia limitato a una tabella reale e non all'intera pagina. |
| src\Accyourate.App\AssetsWindow.cs | 104 | RigidWideGrid | Griglia con 11 colonne fisse e almeno 1455 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\BrandedHomeWindow.cs | 29 | LargeMinWidth | MinWidth impostata a 1180 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\BrandedSplashLoginWindow.cs | 51 | LargeMinWidth | MinWidth impostata a 980 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\BrandingCenterWindow.cs | 75 | LargeMinWidth | MinWidth impostata a 1180 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\ChangePasswordWindow.cs | 28 | LargeMinWidth | MinWidth impostata a 1024 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\ControlUnitWindow.cs | 34 | LargeMinWidth | MinWidth impostata a 1024 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\DesignSystemShowcaseWindow.cs | 16 | LargeMinWidth | MinWidth impostata a 1024 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\DiagnosticsWindow.cs | 20 | LargeMinWidth | MinWidth impostata a 1024 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\DocumentManagementWindow.cs | 41 | LargeMinWidth | MinWidth impostata a 1180 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\DocumentManagementWindow.cs | 63 | HorizontalAutoScroll | Scorrimento orizzontale automatico presente. | Verificare che sia limitato a una tabella reale e non all'intera pagina. |
| src\Accyourate.App\DocumentManagementWindow.cs | 98 | RigidWideGrid | Griglia con 10 colonne fisse e almeno 1490 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\DocumentManagementWindow.cs | 219 | RigidWideGrid | Griglia con 10 colonne fisse e almeno 1200 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\DocumentManagementWindow.cs | 234 | RigidWideGrid | Griglia con 10 colonne fisse e almeno 1200 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\EmployeeDetailWindow.cs | 20 | LargeMinWidth | MinWidth impostata a 1024 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\EmployeeEditWindow.cs | 37 | LargeMinWidth | MinWidth impostata a 1024 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\EmployeesWindow.cs | 39 | LargeMinWidth | MinWidth impostata a 1180 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\EmployeesWindow.cs | 53 | HorizontalAutoScroll | Scorrimento orizzontale automatico presente. | Verificare che sia limitato a una tabella reale e non all'intera pagina. |
| src\Accyourate.App\EmployeesWindow.cs | 94 | RigidWideGrid | Griglia con 9 colonne fisse e almeno 1245 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\EnterpriseDashboardWindow.cs | 22 | LargeMinWidth | MinWidth impostata a 1024 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\EnterpriseDashboardWindow.cs | 36 | HorizontalAutoScroll | Scorrimento orizzontale automatico presente. | Verificare che sia limitato a una tabella reale e non all'intera pagina. |
| src\Accyourate.App\EnterpriseShellFoundationWindow.cs | 17 | LargeMinWidth | MinWidth impostata a 1100 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\EnterpriseTopBarWindow.cs | 22 | LargeMinWidth | MinWidth impostata a 960 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\EnterpriseUxCenterWindow.cs | 14 | LargeMinWidth | MinWidth impostata a 900 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\EnterpriseWorkspaceWindow.cs | 63 | LargeMinWidth | MinWidth impostata a 1180 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\EnterpriseWorkspaceWindow.cs | 131 | RigidWideGrid | Griglia con 7 colonne fisse e almeno 1130 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\GlobalSearchWindow.cs | 22 | LargeMinWidth | MinWidth impostata a 1024 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\InfrastructureWindow.cs | 28 | LargeMinWidth | MinWidth impostata a 1024 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\InfrastructureWindow.cs | 42 | HorizontalAutoScroll | Scorrimento orizzontale automatico presente. | Verificare che sia limitato a una tabella reale e non all'intera pagina. |
| src\Accyourate.App\LaundryMaintenanceWindow.cs | 47 | LargeMinWidth | MinWidth impostata a 1180 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\LaundryMaintenanceWindow.cs | 67 | HorizontalAutoScroll | Scorrimento orizzontale automatico presente. | Verificare che sia limitato a una tabella reale e non all'intera pagina. |
| src\Accyourate.App\LaundryMaintenanceWindow.cs | 103 | RigidWideGrid | Griglia con 8 colonne fisse e almeno 1090 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\LaundryMaintenanceWindow.cs | 145 | RigidWideGrid | Griglia con 10 colonne fisse e almeno 1420 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\LaundryMaintenanceWindow.cs | 244 | RigidWideGrid | Griglia con 8 colonne fisse e almeno 990 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\LaundryMaintenanceWindow.cs | 257 | RigidWideGrid | Griglia con 8 colonne fisse e almeno 990 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\LaundryMaintenanceWindow.cs | 278 | RigidWideGrid | Griglia con 8 colonne fisse e almeno 1100 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\LaundryMaintenanceWindow.cs | 291 | RigidWideGrid | Griglia con 8 colonne fisse e almeno 1100 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\LoginWindow.cs | 27 | LargeMinWidth | MinWidth impostata a 1024 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\MainWindow.cs | 61 | LargeMinWidth | MinWidth impostata a 1120 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\MedicalDevicesWindow.cs | 39 | LargeMinWidth | MinWidth impostata a 1180 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\MedicalDevicesWindow.cs | 53 | HorizontalAutoScroll | Scorrimento orizzontale automatico presente. | Verificare che sia limitato a una tabella reale e non all'intera pagina. |
| src\Accyourate.App\MedicalDevicesWindow.cs | 94 | RigidWideGrid | Griglia con 10 colonne fisse e almeno 1275 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\MedicalDeviceTwinWindow.cs | 22 | LargeMinWidth | MinWidth impostata a 1024 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\MedicalDeviceTwinWindow.cs | 35 | HorizontalAutoScroll | Scorrimento orizzontale automatico presente. | Verificare che sia limitato a una tabella reale e non all'intera pagina. |
| src\Accyourate.App\NotificationsWindow.cs | 13 | LargeMinWidth | MinWidth impostata a 900 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\ProductionQualityWindow.cs | 43 | LargeMinWidth | MinWidth impostata a 1180 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\ProductionQualityWindow.cs | 59 | HorizontalAutoScroll | Scorrimento orizzontale automatico presente. | Verificare che sia limitato a una tabella reale e non all'intera pagina. |
| src\Accyourate.App\ProductionQualityWindow.cs | 91 | RigidWideGrid | Griglia con 10 colonne fisse e almeno 1300 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\ProductionQualityWindow.cs | 154 | RigidWideGrid | Griglia con 8 colonne fisse e almeno 940 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\ProductionQualityWindow.cs | 159 | RigidWideGrid | Griglia con 8 colonne fisse e almeno 940 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\ProductionQualityWindow.cs | 172 | RigidWideGrid | Griglia con 9 colonne fisse e almeno 1110 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\ProductionQualityWindow.cs | 177 | RigidWideGrid | Griglia con 9 colonne fisse e almeno 1110 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\SettingsWindow.cs | 22 | LargeMinWidth | MinWidth impostata a 1024 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\SettingsWindow.cs | 36 | HorizontalAutoScroll | Scorrimento orizzontale automatico presente. | Verificare che sia limitato a una tabella reale e non all'intera pagina. |
| src\Accyourate.App\Shared\UI\ResponsiveUi.cs | 9 | LargeMinWidth | MinWidth impostata a 1024 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\Shared\UI\ResponsiveUi.cs | 26 | HorizontalAutoScroll | Scorrimento orizzontale automatico presente. | Verificare che sia limitato a una tabella reale e non all'intera pagina. |
| src\Accyourate.App\TextileItemWindow.cs | 37 | LargeMinWidth | MinWidth impostata a 1024 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\ThemePersonalizationWindow.cs | 46 | LargeMinWidth | MinWidth impostata a 980 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\UsersWindow.cs | 30 | LargeMinWidth | MinWidth impostata a 1024 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\UsersWindow.cs | 44 | HorizontalAutoScroll | Scorrimento orizzontale automatico presente. | Verificare che sia limitato a una tabella reale e non all'intera pagina. |
| src\Accyourate.App\WarehouseLogisticsWindow.cs | 51 | LargeMinWidth | MinWidth impostata a 1180 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\WarehouseLogisticsWindow.cs | 72 | HorizontalAutoScroll | Scorrimento orizzontale automatico presente. | Verificare che sia limitato a una tabella reale e non all'intera pagina. |
| src\Accyourate.App\WarehouseLogisticsWindow.cs | 136 | RigidWideGrid | Griglia con 7 colonne fisse e almeno 1050 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\WarehouseLogisticsWindow.cs | 171 | RigidWideGrid | Griglia con 7 colonne fisse e almeno 990 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\WarehouseLogisticsWindow.cs | 310 | RigidWideGrid | Griglia con 8 colonne fisse e almeno 1080 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\WarehouseLogisticsWindow.cs | 323 | RigidWideGrid | Griglia con 8 colonne fisse e almeno 1080 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\WarehouseLogisticsWindow.cs | 344 | RigidWideGrid | Griglia con 8 colonne fisse e almeno 980 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\WarehouseLogisticsWindow.cs | 357 | RigidWideGrid | Griglia con 8 colonne fisse e almeno 980 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\WorkflowWindow.cs | 23 | LargeMinWidth | MinWidth impostata a 1024 px. | Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive. |
| src\Accyourate.App\WorkflowWindow.cs | 37 | HorizontalAutoScroll | Scorrimento orizzontale automatico presente. | Verificare che sia limitato a una tabella reale e non all'intera pagina. |
| src\Accyourate.App\WorkflowWindow.cs | 113 | RigidWideGrid | Griglia con 8 colonne fisse e almeno 1140 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |
| src\Accyourate.App\WorkflowWindow.cs | 134 | RigidWideGrid | Griglia con 8 colonne fisse e almeno 1140 px. | Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato. |

## Come interpretare il report

- High: probabile contenuto nascosto o finestra non ridimensionabile.
- Medium: layout da verificare visivamente prima della migrazione.
- Low: schema non responsive che puo essere corretto quando contiene comandi o filtri.

Il CSV contiene tutte le segnalazioni, comprese Medium e Low.
