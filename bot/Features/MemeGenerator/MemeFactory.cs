using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using CoreHtmlToImage;
using HandlebarsDotNet;
using GrapeCity.Documents.Html;
using GrapeCity.Documents.Imaging;
using GrapeCity.Documents.Drawing;

namespace bot.Features.MemeGenerator
{
    public abstract class SocialMediaDestination
    {
        public abstract int MemeImageWidth { get; set; }
        public abstract int MemeImageHeight { get; set; }
        public abstract string TemplateName { get; }
        public abstract string MemeText { get; set; }
        
        private string getrandomfile2(string path)
        {
            string file = null;
            if (string.IsNullOrEmpty(path)) return file;
            var extensions = new string[] { ".png", ".jpg", ".gif" };
            try
            {
                var di = new DirectoryInfo(path);
                var rgFiles = di.GetFiles("*.*").Where( f => extensions.Contains( f.Extension.ToLower()));
                var r = new Random();
                var fileInfos = rgFiles.ToList();
                file = fileInfos.ElementAt(r.Next(0, fileInfos.Count)).Name;
            }
            // probably should only catch specific exceptions
            // throwable by the above methods.
            catch {}
            return file;
        }

        public virtual string ImageUrl {
            get
            {
                var url = getrandomfile2("Images");
                return $"Images/{url}";
            }
        }

        public virtual string Html()
        {
            return "";
        }

        protected string LoadTemplate()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"bot.Features.MemeGenerator.Templates.{TemplateName}.html";
            using var stream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream);
            var result = reader.ReadToEnd();
            var template = Handlebars.Compile(result);
            
            return template(this);
        }
    }

    /// <inheritdoc />
    public class TwitterMediaDestination : SocialMediaDestination
    {
        public override int MemeImageWidth { get; set; } = 1200;
        public override int MemeImageHeight { get; set; } = 1200;
        public override string TemplateName { get; } = "TwitterMeme";
        public override string MemeText { get; set; }

        public override string Html()
        {
            return this.LoadTemplate();
        }
    }
    
    public class MemeFactory<T> where T : SocialMediaDestination
    {
        private readonly SocialMediaDestination _mediaDestination;
        
        public MemeFactory()
        {
            _mediaDestination = (T)Activator.CreateInstance(typeof(T));
        }

        public MemeFactory(T mediaDestination)
        {
            _mediaDestination = mediaDestination;
        }
        
        public void Generate()
        {
            var converter = new HtmlConverter();
            var html = _mediaDestination.Html(); //"<div><strong>Hello</strong> World!</div>";
            var bytes = converter.FromHtmlString(html);
            File.WriteAllBytes("image.jpg", bytes);

            using var re1 = new GcHtmlRenderer(html);
            var jpegSettings1 = new JpegSettings
            {
                DefaultBackgroundColor = System.Drawing.Color.Transparent, 
                WindowSize = new Size(1024, 512)
            };
            //Finally, render the string to an image using RenderToJpeg method of GcHtmlRenderer
            re1.RenderToJpeg("invoice.jpeg", jpegSettings1);
        }
    }
}