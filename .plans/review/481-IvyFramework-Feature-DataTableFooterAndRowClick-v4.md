# Review: DataTable Footer Aggregates

## What to verify

- [ ] Run the `DataTableFooterApp` sample and confirm footer row appears at the bottom of the table with "Total: 139" for Qty, "Avg: 414.80" for UnitPrice, and "Total: 21185.00" for Amount
- [ ] Run the `DataTableMultiAggApp` sample and confirm two lines appear in each footer cell (Total and Avg)
- [ ] Verify footer stays visible (sticky) when scrolling the table content
- [ ] Verify footer alignment matches column alignment (right-aligned for numeric columns)
- [ ] Test with DataTable documentation page to confirm the demo-tabs footer example renders correctly
- [ ] Verify empty datasets with footer defined don't cause errors
