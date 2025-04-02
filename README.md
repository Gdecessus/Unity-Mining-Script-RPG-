# Unity-Mining-Script-RPG-

# How to Use it

Now, before we start getting into the mechanics of the script, its important to note that this was created with the intention of working on my own game, that after a while I decided to open source some of the logics that were created prior to the networking of the game.
This script was made with the intention to be used for every kind of 'resource gathering' tasks in a RPG. So, as you can see by looking at the script, nothing is stopping you from changing the sprites from a node to a tree, herb, farming and so on, the mechanic remains the very same!


# INTRO

The mechanics of the mining system are modular, they are are dependent to a skill system just like old MMO's like runescape/tibia.
Most of our important features can be adjusted directly through the inspector, without the need to code, in the inspector you can find virtually all aspects of the mechanics of the script.
Including but not limited to mining chance, skill xp given per succesfull mining, mining stages sprites, VFX and such.
The script depends on a few elements that will require your attention when implementing to your game, firstly, the game counts on a skill system.
T

# SKILL
This skill system(not shared yet) works just like this;
Everytime you sucesfully "Mine" as in your mining chance went through sucesfully, you'll progress in your Mining Skill, only the successful hits are taken as a progression.
The formula as of now for a success hit works like this; 
        *float successChance = baseMineChance + Mathf.Log(skills.miningLevel + 1) / (difficultyModifier * 5);

        *// Clamp the success chance between 0 and 1
        *successChance = Mathf.Clamp(successChance, 0f, 0.8f); // Cap the success chance at 80%

        *// Roll for success
        *bool success = Random.value < successChance;

If the mining hit is successful, it will pass the xp to my method addskillxp and for debugging purposes will display a log; 
            *skills.AddSkillXP("mining", xpReward);
            *Debug.Log($"Adding {xpReward} XP to Mining skill.");

Example of how my Skill class works;

        public class PlayerSkills : MonoBehaviour
        public float miningLevel = 1;
        private float miningXP = 0;
        public void AddSkillXP(string skillName, float xpGain)
    {  
        switch (skillName.ToLower())
        {       
                case "mining":
                AddXP(ref miningXP, ref miningLevel, xpGain);
                break;
        private void AddXP(ref float skillXP, ref float skillLevel, float xpGain)
    {
        skillXP += xpGain;
        
        float requiredXP = FormulaManager.CalculateSkillExperienceToNextLevel(skillLevel);
        if (skillXP >= requiredXP)
        {
            skillXP = 0;
            skillLevel++;
            Debug.Log($"{GetSkillName(skillLevel)} leveled up to {skillLevel}!");
        }

Pay attention to the part where I call a formula manager class, that's where you'll do your formula to how much xp is needed, you can also do it all in one class, but for good practice I made a class only for formulas, its easier to maintain.
        
# REWARD

Now when talking about what the rewards you have to options, you can either make a small customization (I had this done prior) that will instead of place the item in the player inventory, it will drop the item below the player and then he can decide wether to collect it or not. The second option is for you to add it directly to inventory after performing some checks. Regardless of your choice, you'll have to modify the GrantRewards() method. I left the method commented out and with my own calls to my inventory to check if the inventory has slots available, then create a item and add it to the inventory.

#VFX

I used a VFX that is a particle system with sparks using URP. so either a sucessful hit or a failed hit will still produces the same VFX and the audio that comes with the VFX.

# RESPAWN/DESPAWN

In the inspector we do set a timer to how long we do want our mining node to be 'unavailable', its important to note, that even though I had the chance of removing the GO all together from the scene, I found that rather than removing the GO, it would make more sense in a garbage collection sense and optimization of code/logic to keep it in a non active state with a 'degraded' sprite. in the inspector you'll be able to select 3 stages, youre free to add more as you need, its pretty straight forward.

# RAY TRACING

Probably the biggest challange was to come up with a way to check if the player was actually facing the node in order to mine. I tried using different systems like a box collider as a child object and a script to always adjust based on keyboard input, then later with controller input, and a few other settings that unfortunately didnt work for me.
I then, decided why not just everytime the player try to mine we use ray tracing to throw a 'ray' in the forward position and if it hits a GO that has a layer 'ore' it will allow the mechanics to identify if the player is facing or not a node.

            
