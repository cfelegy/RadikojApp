﻿using System.ComponentModel.DataAnnotations;

namespace Radikoj.Models.AccountViewModels
{
    public class SignInViewModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email {  get; set; }
        
        [DataType(DataType.Password)]
        public string? Token { get; set; }
    }
}
