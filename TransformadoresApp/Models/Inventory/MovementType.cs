namespace TransformadoresApp.Models.Inventory
{
    public enum MovementType
    {
        InitialLoad = 1,
        Purchase = 2,
        Sale = 3,
        ProductionConsumption = 4,
        ProductionOutput = 5,
        InventoryAdjustment = 6,
        TransferIn = 7,
        TransferOut = 8
    }
}
