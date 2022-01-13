﻿using System;

namespace LT.DigitalOffice.TimeService.Models.Dto.Requests
{
  public record CreateWorkTimeRequest
  {
    public float? Hours { get; set; }
    public string Description { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public sbyte Offset { get; set; }
  }
}
