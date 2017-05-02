/*
 * Creado por SharpDevelop.
 * Usuario: hernani
 * Fecha: 05/11/2016
 * Hora: 21:28
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;
using System.Drawing;
using ffmpegUI;

namespace ManagerCG
{
	/// <summary>
	/// Description of itemMovie.
	/// </summary>
	public class itemMovie
	{	
		public itemMovie(string namefile)
		{
			NameFile = namefile;
			Converter con=new Converter();
            VideoFile videofile = con.GetVideoInfo(namefile);

            //OutputPackage outputpack = con.ConvertToFLV(videofile);
            //GetThumbnail(outputpack.PreviewImage);

            Time = Math.Round(TimeSpan.FromTicks(videofile.Duration.Ticks).TotalSeconds, 0);
			Frames = 23;
			Ratio = 2;
		}
        public Image Thumbs { get; set; }=new Bitmap(100,100);
		public string NameFile{get;set;}
		public double Time{get;set;}
		public long Frames{get;set;}
		public int Ratio{get;set;}
	    public bool GetthumbAbort { get; set; } = false;
        private bool ThumbnailCallback()
        {
            return GetthumbAbort;
        }
        private void GetThumbnail(Image imagen)
        {
            if (imagen == null) return;
            Image.GetThumbnailImageAbort callback =
                new Image.GetThumbnailImageAbort(ThumbnailCallback);
            Image image = new Bitmap(imagen);
            Thumbs = image.GetThumbnailImage(100, 100, callback, new
               IntPtr());
            
        }
    }
}
