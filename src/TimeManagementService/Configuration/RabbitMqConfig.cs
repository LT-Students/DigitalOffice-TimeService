﻿using LT.DigitalOffice.Kernel.Broker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LT.DigitalOffice.TimeManagementService.Configuration
{
    public class RabbitMqConfig : BaseRabbitMqOptions
    {
        public string AuthenticationServiceValidationUrl { get; set; }
    }
}
