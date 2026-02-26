using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CardBuilder : MonoBehaviour
{// Lotsa TMP objects that need filling in...will need to add smth for the portraits too.
    [SerializeField] TextMeshProUGUI stringName, stringHealth, stringSpeed, stringDamage, stringDefence;
    [SerializeField] GameObject[] gridSquares; //In inspector should be ordered 0.0, 1.0, 2.0, 0.1 ect...

    public void BuildCard(string Name, int Health, int Speed, int Damage, int Defence, List<Vector2Int> Grid)
    {// Probably a nicer way to do this... Oh well...
        stringName.text = Name;
        stringHealth.text = Health.ToString();
        stringSpeed.text = Speed.ToString();
        stringDamage.text = Damage.ToString();
        stringDefence.text = Defence.ToString();

        //I'm not arsed to do something smart rn...will fix this later...maybe...
        //Just wakes up the right squares. Make sure they're all inactive in the prefab before running the game...
        
        foreach (Vector2Int coord in Grid)
        {// Simple logic tree to find out which squares should show up...inelegant but robust enough...
            int x = coord.x;
            int y = coord.y;

            if (y == 0)
            {
                if (x == 0)
                    gridSquares[0].SetActive(true);
                else if (x == 1)
                    gridSquares[1].SetActive(true);
                else if (x == 2)
                    gridSquares[2].SetActive(true);
            }
            else if (y == 1)
            {
                if (x == 0)
                    gridSquares[3].SetActive(true);
                else if (x == 1)
                    gridSquares[4].SetActive(true);
                else if (x == 2)
                    gridSquares[5].SetActive(true);
            }
            else if (y == 2)
            {
                if (x == 0)
                    gridSquares[6].SetActive(true);
                else if (x == 1)
                    gridSquares[7].SetActive(true);
                else if (x == 2)
                    gridSquares[8].SetActive(true);
            }
        }
    }
}
