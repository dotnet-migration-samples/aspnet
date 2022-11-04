using eShopWebForms.Models;
using System;
using System.Web;

namespace eShopWebForms.Services
{
    public interface IImageService: IDisposable
    {
        string UploadTempImage(HttpPostedFile file, int? catalogItemId);
        string BaseUrl();
        void UpdateImage(CatalogItem item);
        string UrlDefaultImage();
        string BuildUrlImage(CatalogItem item);
        void InitializeCatalogImages();

    }
}
