﻿namespace LT.DigitalOffice.TimeService.Mappers.Interfaces
{
    /// <summary>
    /// Represents interface for a mapper in mapper pattern.
    /// Provides methods for converting an object of <see cref="TIn"/> type into an object of <see cref="TOut"/> type according to some rule.
    /// </summary>
    /// <typeparam name="TIn">Incoming object type.</typeparam>
    /// <typeparam name="TOut">Outgoing object type.</typeparam>
    public interface IMapper<TIn, TOut>
    {
        /// <summary>
        /// Convert an object of <see cref="TIn"/> type into an object of <see cref="TOut"/> type according to some rule.
        /// </summary>
        /// <param name="value">Specified object of <see cref="TIn"/> type.</param>
        /// <returns>The conversion result of <see cref="TOut"/> type.</returns>
        TOut Map(TIn value);
    }
}
