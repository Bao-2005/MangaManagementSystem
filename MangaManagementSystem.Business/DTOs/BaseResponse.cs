using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehouseService.Application.DTOs
{
    public class BaseResponse
    {
        public object? Data { get; set; }
        public string? Message { get; set; }
    }
}
