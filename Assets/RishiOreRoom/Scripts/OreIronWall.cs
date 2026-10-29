using System.Globalization;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
namespace Rishi.OreRoom {
// Updates the existing inventory wall only; creates no UI or construction behavior.
[DefaultExecutionOrder(5000)]
public class OreIronWall : MonoBehaviour {
    public OreHubIntegration hub;
    public OreRoom room;
    public TMP_Text inventoryWall;
    float nextSearch;
    bool warned;
    static readonly Regex IronRow=new Regex(
        @"(?im)^(?<leading>[ \t]*(?:<[^>\r\n]+>[ \t]*)*)(?<name>Iron|Ore)(?![\p{L}\p{N}_])(?<gap>[ \t]*(?:<[^>\r\n]+>[ \t]*)*(?::|=)?[ \t]*(?:<[^>\r\n]+>[ \t]*)*)(?<amount>[+-]?\d+(?:[.,]\d+)?|#)?");
    void LateUpdate() {
        // The real team hub owns the entire inventory wall and includes Iron itself now.
        if(room && room.SharedInventory.Hub)return;
        if(hub && hub.room && hub.room.SharedInventory.Hub)return;
        var inventory=hub?hub.Inventory:room?room.SharedInventory:null;
        if(inventory==null)return;
        // The demo hub already owns its updater. Do not race it from the room too.
        if(!hub && room && room.hub)return;
        if(!inventoryWall) {
            if(Time.unscaledTime<nextSearch)return;
            nextSearch=Time.unscaledTime+1;
            BindExistingWall();
        }
        if(!inventoryWall)return;
        string current=inventoryWall.text;
        string updated=FormatIron(current,inventory.Iron);
        if(updated!=current)inventoryWall.text=updated;
    }
    void BindExistingWall() {
        var board=GameObject.Find("Hub layout copy/Inventory");
        if(!board)board=GameObject.Find("Inventory");
        if(board && board.scene==gameObject.scene) {
            foreach(var label in board.GetComponentsInChildren<TMP_Text>(true)) {
                if(!IronRow.IsMatch(label.text??""))continue;
                inventoryWall=label;return;
            }
        }
        if(warned)return;warned=true;
        Debug.LogWarning("Ore Iron wall: could not find the existing Inventory wall's Ore/Iron row. Assign its TextMeshPro text to Inventory Wall on Ore Iron Wall. No replacement UI was created.",this);
    }
    // Only the matching label/amount changes. Other rows, styling and spacing are retained.
    public static string FormatIron(string current,int amount) {
        if(string.IsNullOrEmpty(current))return current;
        return IronRow.Replace(current,match=> {
            string gap=match.Groups["gap"].Value;
            if(gap.Length==0)gap=": ";
            else if(gap[gap.Length-1]=='>' || gap[gap.Length-1]==':' || gap[gap.Length-1]=='=')gap+=" ";
            return match.Groups["leading"].Value+"Iron"+gap+amount.ToString(CultureInfo.InvariantCulture);
        },1);
    }
}
}
