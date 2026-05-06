using System.ComponentModel.DataAnnotations;
using BeautySalonAPI.Models;

namespace BeautySalonAPI.DTOs.Admin;

public record UpdateStatusDto([Required] OrderStatus Status);
