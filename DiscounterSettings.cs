using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;

namespace Discounter;

public class DiscounterSettings : ISettings
{
    //Mandatory setting to allow enabling/disabling your plugin
    public ToggleNode Enable { get; set; } = new ToggleNode(false);

    //Put all your settings here if you can.
    //There's a bunch of ready-made setting nodes,
    //nested menu support and even custom callbacks are supported.
    //If you want to override DrawSettings instead, you better have a very good reason.
    public RangeNode<int> DiscountPercent { get; set; } = new(15, 0, 100);
    public RangeNode<int> ExtraDelay { get; set; } = new(20, 0, 100);
}