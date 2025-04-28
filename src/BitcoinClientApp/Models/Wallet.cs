using System;
using System.ComponentModel.DataAnnotations;

namespace BitcoinClientApp.Models
{
    public class Wallet
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string EncryptedPrivateKey { get; set; } = string.Empty;

        [Required]
        public string Salt { get; set; } = string.Empty;

        [Required]
        public string PublicKey { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 