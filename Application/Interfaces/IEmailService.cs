using System;
using System.Collections.Generic;
using System.Linq;
//using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Email;

namespace Application.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    Task SendEmailAsync(EmailDto dto);
    //Task SendEmailAsync(string to, string subject, string body, List<Attachment>? attachments = null);
}

//public record Attachment
//{
//    public string FileName { get; init; }
//    public byte[] Content { get; init; }
//    public string ContentType { get; init; }
//}