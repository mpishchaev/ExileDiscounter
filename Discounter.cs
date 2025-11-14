using ExileCore;
using Vector2 = System.Numerics.Vector2;
using ImGuiNET;
using System.Collections.Generic;
using ExileCore.PoEMemory.Elements.InventoryElements;
using ExileCore.Shared;
using ExileCore.Shared.Helpers;
using System.Threading.Tasks;
using System.Windows.Forms;
using System;

namespace Discounter;

public class Discounter : BaseSettingsPlugin<DiscounterSettings>
{    
    private Random _rnd = new Random();
    private SyncTask<bool> _currentOperation;

    public override void Render()
    {               
        if (_currentOperation != null)
        {
             TaskUtils.RunOrRestart(ref _currentOperation, () => null);
             return;
        }
        DrawDiscountButton();
    }

    private void DrawDiscountButton()
    {
        var offlineMerchantPanel = GameController.IngameState.IngameUi.OfflineMerchantPanel;
        if (offlineMerchantPanel == null || offlineMerchantPanel.IsVisible == false) return; 

        var stashPos = offlineMerchantPanel.VisibleStash.GetClientRect();

        // You can also calculate relative to the merchant panel if needed
        var position = new Vector2(stashPos.X, stashPos.Bottom + 7); // 5 pixels padding

        ImGui.SetNextWindowPos(position, ImGuiCond.Always); // Use Always to position it each frame
        if (ImGui.Begin("Discount", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize))
        {
            if (ImGui.Button($"Set {Settings.DiscountPercent.Value}% discount"))
            {
                _currentOperation = DiscountAllCurrentStashItems(offlineMerchantPanel.VisibleStash.VisibleInventoryItems);  
            }
        }
    }

    private async SyncTask<bool> DiscountAllCurrentStashItems(IList<NormalInventoryItem> visibleInventoryItems)
    {
        foreach (var item in visibleInventoryItems)
        {
            if (!await DiscountItem(item))
            {        
                Input.KeyDown(Keys.Escape);
                await Delay(25);
                Input.KeyUp(Keys.Escape);
                await Delay(15);                
            }
            await Delay(75);
        }
        return true;
    }

    private async SyncTask<bool> DiscountItem(NormalInventoryItem item)
    {               
        var price = await OpenPriceWindowAndGetPrice(item);     

        DebugWindow.LogMsg($"Current item price is: {price}");
        if (decimal.TryParse(price, out var originalPrice)) {            
            var discount = Math.Ceiling(originalPrice * (Settings.DiscountPercent / 100m));
            DebugWindow.LogMsg($"Discount is: {discount}");
            var loweredPrice = (int)(originalPrice - discount);  
            if (loweredPrice != 0 && loweredPrice != originalPrice)
            {
                DebugWindow.LogMsg($"Applying discount: {loweredPrice}");
                await ApplyDiscount(item, loweredPrice);    
                return true;
            }
            return false;
        }

        return false;
    }

    private async SyncTask<string> OpenPriceWindowAndGetPrice(NormalInventoryItem item)
    {
        Input.SetCursorPos(item.GetClientRect().Center.ToVector2Num());
        await Delay(75);

        Input.Click(MouseButtons.Right);
        await Delay(75);

        if (GameController.IngameState.IngameUi.PopUpWindow?.IsActive ?? false) {
            var price = GetPrice();
            return price;
        }
        return null;         
    }

    private string GetPrice()
    {
        return GameController.IngameState.IngameUi.PopUpWindow?.Children[2]?.Children[0]?.Children[0]?.Text;
    }

    private async SyncTask<bool> ApplyDiscount(NormalInventoryItem item, int loweredPrice)
    {
        // Type the new price
        string priceStr = loweredPrice.ToString();
        foreach (char c in priceStr)
        {
            Input.KeyDown((Keys)c);
            await Delay(25);
            Input.KeyUp((Keys)c);
            await Delay(15);
        }
        await Delay(50);

        // Press Enter to confirm the price
        Input.KeyDown(Keys.Enter);
        await Delay(25);
        Input.KeyUp(Keys.Enter);
        await Delay(15);

        return true;
    }    

    private async Task Delay(int ms)
    {
        await Task.Delay(ms + _rnd.Next(0, Settings.ExtraDelay));
    }
}