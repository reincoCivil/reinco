using Java.IO;
using Newtonsoft.Json;
using Plugin.Media;
using Reinco.Recursos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Reinco.Entidades
{
    public class SupervisarActividadItem: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string nombrePropiedad)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombrePropiedad));
        }


        WebService Servicio = new WebService();
        string Mensaje;
        VentanaMensaje mensaje = new VentanaMensaje();

        public int idSupervisionActividad { get; set; }
        public string item { get; set; }
        public string actividad { get; set; }
        public bool observacionLevantada { get; set; }
        public string anotacionAdicinal { get; set; }
        public string tolerancia { get; set; }
        public bool aprobacion { get; set; }


        public ICommand guardarItem { get; private set; }
        public ICommand AprobacionCommand { get; private set; }


        public SupervisarActividadItem()
        {
            AprobacionCommand = new Command(() =>
            {
                App.Current.MainPage.DisplayAlert("Alerta", "Me ejecute", "Aceptar");
            });

            #region Lammando A Guardar Actividad
            guardarItem = new Command(() =>
                {
                // App.Current.MainPage.DisplayAlert("Aceptar", this.actividad + this.anotacionAdicinal, "Aceptar");
                GuardarActividad();
                }); 
            #endregion

            #region ================= Expandir =================
            MostrarAnotacion = true;
            ExpandirAnotacion = new Command(() =>
            {
                if (toggle == 0)
                {
                    MostrarAnotacion = true;
                    toggle = 1;
                }
                else
                {
                    MostrarAnotacion = false;
                    toggle = 0;
                }
            }); 
            #endregion

            #region ================ Uso De La Camara ================
            EncenderCamara = new Command(async () =>
                {
                  
                    await CrossMedia.Current.Initialize(); // Inicializando la libreri
                    
                    // Verificando si el dispotivo tiene Camara
                    if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
                    {
                        await App.Current.MainPage.DisplayAlert("Error", ":( No hay cámara disponible.", "Aceptar");
                    }

                // Directorio para almacenar la imagen
                var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
                {
                    Directory = "fotos",
                    Name = "fotoreinco.jpg",
                    AllowCropping = true,
                });

                // mostrando la imagen en la interfas del telefono
                if (file != null)
                {
                    await App.Current.MainPage.DisplayAlert("Localizacion Del Archivo", file.Path, "OK");

                    RutaImagen = ImageSource.FromStream(() =>
                    {
                        var stream = file.GetStream();
                        file.Dispose();
                        return stream;
                    });
                    

                    // Preparando la foto para enviar al webservice
                    var foto = file.GetStream();
                    var fotoArray = ReadFully(foto);

                    var fotos = new Fotos
                    {
                        id = 5,
                        array = fotoArray,
                    };
                       // ImageSource imagenpc = ImageSource.FromFile("D:/rana.jpg");
                   var imagenSerializado = JsonConvert.SerializeObject(fotos);
                    var body = new StringContent(imagenSerializado, Encoding.UTF8, "application/json");
                        string imagen64 = Convert.ToBase64String(fotoArray);
                        object[,] variables = new object[,] {
                            { "idSupervisionActividad",idSupervisionActividad  } ,{ "foto", imagen64 },{ "anotacionAdicional", "asdfasdf" }};

                        dynamic result = await Servicio.MetodoPostStringImagenes("SupervisionActividad.asmx", "guardarAnotacionFoto", variables);
                        Mensaje = Convert.ToString(result);
                        if (result != null)
                        {
                            await App.Current.MainPage.DisplayAlert("Guardar Foto y Anotación", Mensaje, "OK");
                            return;
                        }
                    }

                        //Retratado
                        //var streamTratado = new MemoryStream(fotoArray);
                        //this.ImagenTratado = streamTratado;
                    }
                    // End Camera
                });
            #endregion

        }

        #region ================ Expadir Actividad Por Cada Items Correspondiente ================
        int toggle = 0;
        public ICommand ExpandirAnotacion { get; private set; }
        public bool mostrarAnotacion { get; set; }
        public bool MostrarAnotacion
        {
            set
            {
                if (mostrarAnotacion != value)
                {
                    mostrarAnotacion = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MostrarAnotacion"));
                }
            }
            get
            {
                return mostrarAnotacion;
            }
        }
        #endregion

        #region ================= Preparando la Interaccion De la Camara =================
        // Preparadndo la imagen para enviar


        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }


        public byte [] fotoArray { get; set; } // Array De Bits Para Enviar la Foto Al Web Service

        private ImageSource imagenTratado { get; set; }
        public ImageSource ImagenTratado
        {
            set
            {
                if (imagenTratado != value)
                {
                    imagenTratado = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ImagenTratado"));
                }
            }
            get
            {
                return imagenTratado;
            }
        }
        // InterAccion Con la Camara
        private ImageSource rutaImagen;
        public ImageSource RutaImagen
        {
            set
            {
                if (rutaImagen != value)
                {
                    rutaImagen = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RutaImagen"));
                }
            }
            get
            {
                return rutaImagen;
            }
        }
        public ICommand EncenderCamara { get; private set; }
        #endregion
        #region==================base64 a imagen=============================
        private string imageBase64;
        public string ImageBase64
        {
            get { return imageBase64; }
            set
            {
                imageBase64 = value;
                OnPropertyChanged("ImageBase64");

                Image = Xamarin.Forms.ImageSource.FromStream(
                    () => new MemoryStream(Convert.FromBase64String(imageBase64)));
            }
        }

        private Xamarin.Forms.ImageSource image;
        public Xamarin.Forms.ImageSource Image
        {
            get { return image; }
            set
            {
                image = value;
                OnPropertyChanged("Image");
            }
        }
        #endregion
        #region===========imagen a base64======================

        //var base64Img = new Base64Image
        //{
        //    FileContents = File.ReadAllBytes("Path to image"),
        //    ContentType = "image/png"
        //};

        //string base64EncodedImg = base64Img.ToString();
        

        #endregion
        #region ===================== Guardar Actividad =====================
        public async void GuardarActividad()
        {
            try
            {
                int Si, No;
                if (aprobacion == false)
                {
                    Si = 0;
                    No = 1;
                }
                else
                {
                    Si = 1;
                    No = 0;
                }
                object[,] variables = new object[,] {
                        { "idSupervisionActividad",idSupervisionActividad  } ,{ "si", Si },{ "no", No },
                        { "observacionLevantada", No }, { "anotacionAdicional", anotacionAdicinal==null?"":anotacionAdicinal }};

                dynamic result = await Servicio.MetodoGetString("SupervisionActividad.asmx", "guardarSupervisionActividad", variables);
                Mensaje = Convert.ToString(result);
                if (result != null)
                {
                    await App.Current.MainPage.DisplayAlert("Guardar actividad", Mensaje, "OK");
                    return;
                }
            }
            catch (Exception ex)
            {
                await mensaje.MostrarMensaje("Agregar Usuario", "Error en el dispositivo o URL incorrecto: " + ex.ToString());
            }
        }
        #endregion
        public void Imagen64()
        {
            //using (Image image = Image.FromFile(Path))
            //{
            //    using (MemoryStream m = new MemoryStream())
            //    {
            //        image.Save(m, image.RawFormat);
            //        byte[] imageBytes = m.ToArray();

            //        // Convert byte[] to Base64 String
            //        string base64String = Convert.ToBase64String(imageBytes);
            //        return base64String;
            //    }
            //}
        }
    }
}
