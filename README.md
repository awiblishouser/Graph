# Graph
Accepts lists of To, CC, BCC, and file attachments using Microsoft Graph

ChatGPT said:

EmailService Class

The EmailService class provides a reusable integration with the Microsoft Graph API to send emails from your ASP.NET Core MVC application. It supports sending messages to single or multiple recipients, adding CC/BCC addresses, and attaching files.

This service uses the Azure Identity library to authenticate with Microsoft Graph using a Client Secret Credential. You will need to configure your Azure AD app registration credentials in appsettings.json and register the service in Program.cs if you haven’t already.

Key Features

Single & Bulk Email Sending – Send emails to one or multiple recipients.

HTML Email Support – Messages are sent with full HTML formatting.

CC & BCC Support – Include carbon copy and blind carbon copy recipients.

File Attachments – Attach one or more files with automatic MIME type detection.

Microsoft Graph Integration – Uses the latest Graph SDK for secure and scalable communication.

Dependency Injection Ready – Designed to be registered and used as a service in ASP.NET Core MVC.

Authentication

Uses Azure.Identity.ClientSecretCredential for secure, app-based authentication.

Requires TenantId, ClientId, and ClientSecret to be configured in appsettings.json.

Example Usage

// Injected via constructor
private readonly EmailService _emailService;

await _emailService.SendEmailAsync(
fromAddress: "sender@domain.com
",
toAddress: "recipient@domain.com
",
subject: "Welcome to Lillie's Toy Box!",
body: "<h1>Thank you for signing up!</h1>",
user: "ccuser@domain.com
"
);

Requirements

Azure AD application registered with Mail.Send permissions.

Properly configured appsettings.json for Graph credentials.

Program.cs must register the EmailService and bind configuration.
