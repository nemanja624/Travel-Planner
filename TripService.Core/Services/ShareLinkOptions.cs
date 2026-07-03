namespace TripService.Core.Services;

public sealed class ShareLinkOptions
{
    public string PublicBaseUrl { get; set; } = "http://localhost:3000";

    public string QrCodeUrlTemplate { get; set; } = "https://api.qrserver.com/v1/create-qr-code/?size=240x240&data={shareUrl}";
}
