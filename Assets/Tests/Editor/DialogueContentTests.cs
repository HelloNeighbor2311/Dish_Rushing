using NUnit.Framework;
using UnityEditor;

/// <summary>Content checks: re-run after editing the dialogue assets.</summary>
public class DialogueContentTests
{
    private const int MaxLineLength = 150;
    private const int MaxNameLength = 14;

    [Test]
    public void IntroContent_AllLinesFit()
    {
        var intro = AssetDatabase.LoadAssetAtPath<IntroContentSO>("Assets/Data/IntroContent.asset");
        Assert.IsNotNull(intro);
        Assert.AreEqual(6, intro.scenes.Length);
        foreach (IntroScene scene in intro.scenes)
        {
            Assert.IsNotNull(scene.background, scene.title);
            foreach (IntroLine line in scene.lines)
            {
                Assert.That(line.text, Is.Not.Empty);
                Assert.LessOrEqual(line.text.Length, MaxLineLength, line.text);
                Assert.IsTrue(line.speakerKey == IntroContentSO.NarratorKey || intro.TryGetCharacter(line.speakerKey, out _),
                    line.speakerKey);
                if (!string.IsNullOrEmpty(line.actorKey))
                    Assert.IsTrue(intro.TryGetCharacter(line.actorKey, out _), line.actorKey);
            }
        }
    }

    [Test]
    public void IntroContent_CharacterNamesFitNameTag()
    {
        var intro = AssetDatabase.LoadAssetAtPath<IntroContentSO>("Assets/Data/IntroContent.asset");
        foreach (IntroCharacter character in intro.characters)
        {
            Assert.LessOrEqual(character.displayName.Length, MaxNameLength, character.displayName);
            Assert.IsNotNull(character.portrait, character.key);
            Assert.AreEqual(5, character.idleFrames.Length, character.key);
        }
    }

    [Test]
    public void OwnerDialogue_AllLinesFit()
    {
        var d = AssetDatabase.LoadAssetAtPath<OwnerDialogueSO>("Assets/Data/OwnerDialogue.asset");
        Assert.AreEqual(7, d.stepObjectives.Length);
        Assert.AreEqual(8, d.tips.Length);
        Assert.AreEqual(5, d.stepIntro.Length);
        Assert.AreEqual(5, d.stepIdle.Length);
        Assert.AreEqual(5, d.stepCompleted.Length);
        foreach (StepLines step in d.stepIdle)
        {
            Assert.LessOrEqual(step.lines.Length, 3);
            foreach (string text in step.lines) Assert.LessOrEqual(text.Length, MaxLineLength, text);
        }
        foreach (string[] group in new[]
                 {
                     d.welcome, d.startShift, d.stepObjectives, d.talkPrompts, d.tips, d.barkBroken, d.barkDirty,
                     d.barkPileUp, d.barkTimeLow, d.barkCleanStreak, d.dishBrokenDuringTutorial, d.winShift,
                 })
        {
            Assert.IsNotEmpty(group);
            foreach (string text in group)
            {
                Assert.That(text, Is.Not.Empty);
                Assert.LessOrEqual(text.Length, MaxLineLength, text);
            }
        }
        foreach (string text in new[]
                 {
                     d.scrubDirtyReaction, d.tipsEasterEgg, d.replayAck, d.loseTime, d.loseBroken, d.chefEndShift,
                 })
        {
            Assert.That(text, Is.Not.Empty);
            Assert.LessOrEqual(text.Length, MaxLineLength, text);
        }
    }
}
