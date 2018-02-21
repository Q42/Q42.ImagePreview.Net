# Q42.ImagePreview.Net

Server & Client component for creating and rendering ~200 byte images (25% of original preview size).

Look at [this blog post](https://blog.q42.nl/the-imagepreview-library-render-previews-with-only-200-bytes-of-image-data-312fb281d081) for more details.

For the iOS clients.

## Install

Include the NuGet package

    Install-Package Q42.ImagePreview
    
## Create the images

    var image = Image.FromFile("[path to your image]");
    var body = ImagePreviewConverter.CreateImagePreview(image);

Store the body (`byte[]`). This is the information that you can send to your clients.

## Render the image from the body

    ImagePreviewConverter.Base64ImageFromBody(body)
    
Don't forget to add a blur to your images.
