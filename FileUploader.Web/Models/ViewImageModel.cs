using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileUploader.Data;

namespace FileUploader.Web.Models
{
    public class ViewImageModel
    {
        public bool HasPermission { get; set; }
        public Image Image { get; set; }
        public string Message { get; set; }
    }
}
