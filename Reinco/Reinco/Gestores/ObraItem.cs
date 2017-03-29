﻿using GalaSoft.MvvmLight.Command;
using Reinco.Interfaces.Obra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Reinco.Gestores
{
    public class ObraItem
    {
        public string idObra { get; set; }
        public string nombre { get; set; }
        public string codigo { get; set; }
        public string responsable { get; set; }
        public string plantilla { get; set; }




        public ICommand asignarPlantilla { get; private set; }
        
        public ObraItem()
        {
            asignarPlantilla = new Command(() =>
            {
                //App.ListarObra.Navigation.PushAsync(new AsignarPlantilla());
                App.ListarObra.Navigation.PushAsync(new PaginaPrueva(this.idObra,this.nombre,this.codigo));
            });
        }




    }
}
