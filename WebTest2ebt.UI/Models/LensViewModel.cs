﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebTest2ebt.UI.Models
{
    public class LensViewModel : EquipmentViewModel
    {
        [Required]
        public int MyProperty { get; set; }
    }
}
