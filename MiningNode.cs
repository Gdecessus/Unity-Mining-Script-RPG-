using Esper.Inventool.Items; 
using UnityEngine;
using System.Collections;
using Esper.Inventool;
using static FormulaManager;

[RequireComponent(typeof(Collider2D))]
public class MiningNode : MonoBehaviour
{
    [Header("Mining Node Properties")]
    public string nodeName = "Ore Node"; 
    [Range(0, 1)] public float baseMineChance = 0.4f; 
    public float xpReward = 10f; 
    public float difficultyModifier = 5f; 
    public int maxMiningStages = 3; 

    [Header("Mining Stages (Set Sprites in Inspector)")]
    public Sprite[] miningStageSprites; 
    private int currentMiningStage = 0; 

    [Header("Rewards")]
    public Item oreItem; 
    public int minOreYield = 1; 
    public int maxOreYield = 3; 

    [Header("VFX and Respawn")]
    public GameObject mineVFX; 
    public float minRespawnTime = 30f; 
    public float maxRespawnTime = 120f;

    [Header("Cooldown")]
    public float exhaustCooldown = 5f; //seconds

    [Header("Raycast Settings")]
    public float miningRange = 2f; 
    public LayerMask oreLayer; // layerMask for the ore nodes (play around this)

    private bool isMined = false; 
    private bool isOnCooldown = false; 
    private SpriteRenderer spriteRenderer; 
    private Collider2D nodeCollider;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        nodeCollider = GetComponent<Collider2D>();

        if (spriteRenderer == null)
            Debug.LogError("MiningNode is missing a SpriteRenderer component!");
        if (nodeCollider == null)
            Debug.LogError("MiningNode is missing a Collider2D component!");
        UpdateSprite();
    }

    private void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (isMined || isOnCooldown) return;

            PlayerStats playerStats = FindObjectOfType<PlayerStats>();
            if (playerStats != null && IsPlayerFacingNode(playerStats.transform))
            {
                Debug.Log("Attempting to mine the ore node (Key Press)."); 
                StartCoroutine(TryMine(playerStats));
            }
            else
            {
                Debug.Log("Player is not facing the node.");
            }
        }
    }

    private bool IsPlayerFacingNode(Transform playerTransform)
{
    
    PlayerMovement playerMovement = playerTransform.GetComponent<PlayerMovement>();
    if (playerMovement == null)
    {
        Debug.LogError("No Movement script.");
        return false;
    }

    
    Vector2 playerForward = playerMovement.MovementInput.normalized;

   
    if (playerForward == Vector2.zero)
    {
        playerForward = playerMovement.LastMovementDirection;
    }

    // debugs the player's forward direction
    Debug.Log($"Player Forward: {playerForward}");

    // raycast to always shoot in the frontal position of player to check
    RaycastHit2D hit = Physics2D.Raycast(playerTransform.position, playerForward, miningRange, oreLayer);

    // Debug the ray
    Debug.DrawRay(playerTransform.position, playerForward * miningRange, Color.red, 1f);

    // simple check
    if (hit.collider != null && hit.collider.gameObject == gameObject)
    {
        return true;
    }

    return false;
}
    private IEnumerator TryMine(PlayerStats playerStats)
    {
        PlayerSkills skills = playerStats.playerSkills;

        if (skills == null)
        {
            Debug.LogWarning("PlayerSkills component is missing on the Player.");
            yield break;
        }
        if (isMined)
        {
            Debug.Log("This node has already been mined.");
            yield break; =
        }

        PlayMineVFX();

        float successChance = baseMineChance + Mathf.Log(skills.miningLevel + 1) / (difficultyModifier * 5);

        // Clamp the success chance between 0 and 1
        successChance = Mathf.Clamp(successChance, 0f, 0.8f); // Cap the success chance at 80%

        // Roll for success
        bool success = Random.value < successChance;

        if (success)
        {
            Debug.Log($"Successfully mined {nodeName}!");

            // Pass the XP reward to the player's skill system
            skills.AddSkillXP("mining", xpReward);
            Debug.Log($"Adding {xpReward} XP to Mining skill.");

            // Progress the mining stages
            currentMiningStage++;
            Debug.Log($"Current Mining Stage: {currentMiningStage}"); // Debug log to show current mining stage
            UpdateSprite();
            GrantRewards();

            // If fully mined, grant rewards and start respawn/despawn
            if (currentMiningStage >= maxMiningStages)
            {
                isMined = true; // Mark the node as mined
                Debug.Log($"{nodeName} is fully mined. No further interaction possible.");
                StartCoroutine(RespawnNode());
            }
        }
        else
        {
            Debug.Log("Mining failed.");
        }

        // Start the cooldown timer
        StartCoroutine(StartCooldown());
    }

    private IEnumerator StartCooldown()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(exhaustCooldown);
        isOnCooldown = false;
        Debug.Log($"{nodeName} is no longer on cooldown.");
    }

    private void UpdateSprite()
    {
        // Check if we have a valid sprite array and the current stage is within bounds (check your inspector)
        if (miningStageSprites != null && miningStageSprites.Length > 0 && currentMiningStage < miningStageSprites.Length)
        {
            spriteRenderer.sprite = miningStageSprites[currentMiningStage];
            Debug.Log($"Sprite updated to stage {currentMiningStage}"); // Debug log to confirm sprite update
        }
        else
        {
            Debug.LogWarning("No sprites set for mining stages");
        }
    }

    private void GrantRewards()
    {
        // gives the reward based on the ore selected in the inspector
        Item rewardItem = oreItem;
        int oreYield = Random.Range(minOreYield, maxOreYield + 1);

        Debug.Log($"You obtained {oreYield}x {rewardItem.displayName} from {nodeName}!");

        // Check if the inventory has space, Replace with your actual inventory call
        if (Inventool.Inventory.HasSpaceForItem(rewardItem))
        {
            // Create an ItemStack for the reward, Replace with your actual inventory call
            ItemStack oreStack = new ItemStack(rewardItem, oreYield);
            Inventool.Inventory.AddItems(oreStack);

            Debug.Log($"Added {oreYield}x {rewardItem.displayName} to inventory.");
        }
        else
        {
            Debug.Log("Not enough space in the inventory to add ores.");
        }
    }
    
    private void PlayMineVFX()
    {
        // Instantiate the mining VFX (e.g., sparks)
        if (mineVFX != null)
        {
            Instantiate(mineVFX, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Mine VFX prefab is missing.");
        }
    }
    // node never leaves the scene, stays unminable to avoid unity instantiation garbage collection
    private IEnumerator RespawnNode()
    {
        float respawnTime = Random.Range(minRespawnTime, maxRespawnTime); =
        yield return new WaitForSeconds(respawnTime); =

        // Reset the mining node
        isMined = false;
        currentMiningStage = 0;
        UpdateSprite();
        Debug.Log($"{nodeName} has respawned and is ready to be mined again.");
    }
}
