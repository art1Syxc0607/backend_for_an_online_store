using Application.DTOs.Email;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Services;

public interface IEmailTemplateService
{
    // Auth
    EmailDto CreateRegisterNotificationEmail(User user, string token, string baseUrl, 
        string? returnUrl = null);

    EmailDto ResendEmailConfirmation(User user, string token, string baseUrl);
    EmailDto CreateLoginNotificationEmail(User user, 
        DateTime loginTime, string baseUrl, DeviceInfoDto? deviceInfo = null,
        LocationInfoDto? locationInfo = null);
    EmailDto CreatePasswordResetEmail(User user, string resetCode);

    // Order
    EmailDto CreateOrderConfirmationEmail(Order order, User user, string baseUrl);
    EmailDto CreateShipmentEmail(Order order, User user, string baseUrl);
    EmailDto CreateDeliveryEmail(Order order, User user, string baseUrl);
    EmailDto CreateReceiptEmail(Order order, User user, string baseUrl);

    // Payment
    EmailDto CreatePaymentConfirmationEmail(Order order, User user, Payment payment, string baseUrl);

    // Review
    EmailDto CreateReviewResponseNotificationEmail(Review review, User userm, Product product, string baseUrl);

    // cleanup
    EmailDto CreateOrderCancelledByTimeoutEmail(
        Order order,
        User user,
        string baseUrl);
    
}