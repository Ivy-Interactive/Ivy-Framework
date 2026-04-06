namespace Ivy.Tendril.Services;

public interface IModelPricingService
{
    ModelPricing GetPricing(string modelName);
    CostCalculation CalculateSessionCost(string sessionId);
}
