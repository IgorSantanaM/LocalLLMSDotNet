using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.Text;

namespace AiChatClient.Maui.Services
{
    public class ImageGenerationService(IImageGenerator imageGenerator)
    {
        readonly IImageGenerator _imageGenerator = imageGenerator;

        public async Task<byte[]?> GenerateImageAsync(string prompt, CancellationToken token = default)
        {
            var options = new ImageGenerationOptions
            {
                MediaType = "image/png",
                ImageSize = new System.Drawing.Size(width: 1024, height: 1024),
                Count = 1,
                StreamingCount = 1
            };
           
            var response = await _imageGenerator.GenerateImagesAsync(prompt, options, token);

            var firstImage = response.Contents.OfType<DataContent>().FirstOrDefault();

            return firstImage?.Data.ToArray();
        }
    }
}
