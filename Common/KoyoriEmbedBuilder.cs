namespace Saer.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

/// <summary>
/// A custom Embed builder with a theme.
/// </summary>
internal class KoyoriEmbedBuilder : EmbedBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KoyoriEmbedBuilder"/> class.
    /// </summary>
    public KoyoriEmbedBuilder()
    {
        this.WithColor(new Color(230, 158, 206));
    }
}
