using Application.DTOs.Email;
using Domain.Entities;

public interface IEmailTemplateService
{
    EmailDto CreateConfirmationEmail(User user, string token, string? returnUrl = null);
    EmailDto CreateLoginNotificationEmail(User user, DateTime loginTime, string? deviceInfo = null);
    EmailDto CreateOrderConfirmationEmail(Order order, User user);
    EmailDto CreateShipmentEmail(Order order, User user);
    EmailDto CreateDeliveryEmail(Order order, User user);
    EmailDto CreateReceiptEmail(Order order, User user);
    EmailDto CreatePasswordResetEmail(User user, string token);
}