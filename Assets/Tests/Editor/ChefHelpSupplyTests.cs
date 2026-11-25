using NUnit.Framework;

public class ChefHelpSupplyTests
{
    [Test]
    public void OffersExpireAndCannotBeClaimedTwice()
    {
        var supply = new ChefHelpSupply(new[] { 20f, 70f }, 20f);
        supply.Tick(19f);
        Assert.IsFalse(supply.Claim());
        supply.Tick(20f);
        Assert.IsTrue(supply.Claim());
        Assert.IsFalse(supply.Claim());
        supply.Tick(30f);
        Assert.IsFalse(supply.Claim());
        supply.Tick(70f);
        Assert.IsTrue(supply.Available);
        supply.Tick(90f);
        Assert.IsFalse(supply.Claim());
        Assert.AreEqual(1, supply.Drinks);
        Assert.IsTrue(supply.Consume());
        Assert.IsFalse(supply.Consume());
    }

    [Test]
    public void SkippedWindowsDoNotGrantRewardsAndNewShiftResetsInventory()
    {
        var supply = new ChefHelpSupply(new[] { 20f, 70f }, 20f);
        supply.Tick(60f);
        Assert.IsFalse(supply.Claim());
        supply.Tick(75f);
        Assert.IsTrue(supply.Claim());
        Assert.AreEqual(1, supply.Drinks);
        Assert.AreEqual(0, new ChefHelpSupply(new[] { 20f }, 20f).Drinks);
    }
}
