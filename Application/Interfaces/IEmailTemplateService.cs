using Application.DTOs.Email;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Services;

public interface IEmailTemplateService
{
    EmailDto CreateRegisterNotificationEmail(User user, string token);
    EmailDto CreateLoginNotificationEmail(User user, 
        DateTime loginTime, DeviceInfoDto? deviceInfo = null,
        LocationInfoDto? locationInfo = null);
    EmailDto CreateOrderConfirmationEmail(Order order, User user);
    EmailDto CreateShipmentEmail(Order order, User user);
    EmailDto CreateDeliveryEmail(Order order, User user);
    EmailDto CreateReceiptEmail(Order order, User user);
    EmailDto CreatePasswordResetEmail(User user, string token);
}