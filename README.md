# Q42.ImagePreview.Net

Server & Client component for creating and rendering ~200 byte images (25% of original preview size).

Look at [this blog post](http://q42.com/blog/post/133591843068/the-imagepreview-library-render-previews-with) for more details.

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
