using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Enums;

namespace Infrastructure.Services.Payment;

// Infrastructure/Services/Payment/PaymentStrategyFactory.cs
public interface IPaymentStrategyFactory
{
    IPaymentStrategy GetStrategy(PaymentMethod method);
}

public class PaymentStrategyFactory : IPaymentStrategyFactory
{
    private readonly Dictionary<PaymentMethod, IPaymentStrategy> _strategies;

    // тк стратегии зареганы в DI, DI сам их подставить при создании фабрики
    public PaymentStrategyFactory(IEnumerable<IPaymentStrategy> strategies)
    {
        _strategies = strategies.ToDictionary(s => s.Method);
    }   

    public IPaymentStrategy GetStrategy(PaymentMethod method)
    {
        if (_strategies.TryGetValue(method, out var strategy))
            return strategy;

        throw new ArgumentException($"Unsupported payment method: {method}");
    }
}
