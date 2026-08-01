using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class EndlessLevelHandler : MonoBehaviour
{
    [SerializeField]
    GameObject[] sectionsPrefabs;

    [SerializeField]
    float sectionLength = 100f;

    GameObject[] sectionsPool = new GameObject[20];
    GameObject[] sections = new GameObject[10];

    Transform playerCarTransform;
    WaitForSeconds waitFor100ms = new WaitForSeconds(0.1f);

    void Start()
    {
        playerCarTransform = GameObject.FindGameObjectWithTag("Player").transform;

        int prefabIndex = 0;

        for (int i = 0; i < sectionsPool.Length; i++)
        {
            sectionsPool[i] = Instantiate(sectionsPrefabs[prefabIndex]);
            sectionsPool[i].SetActive(false);

            prefabIndex++;

            if (prefabIndex >= sectionsPrefabs.Length)
            {
                prefabIndex = 0;
            }
        }

        for (int i = 0; i < sections.Length; i++)
        {
            GameObject randomSection = GetRandomSectionFromPool();

            // MODIFICATION : On utilise transform.position pour baser la route sur la position de CE script
            float spawnZ = transform.position.z + (i * sectionLength);
            randomSection.transform.position = new Vector3(transform.position.x, transform.position.y, spawnZ);

            randomSection.SetActive(true);

            sections[i] = randomSection;
        }

        StartCoroutine(UpdateLessOftenCo());
    }

    IEnumerator UpdateLessOftenCo()
    {
        while (true)
        {
            UpdateSectionPositions();
            yield return waitFor100ms;
        }
    }

    void UpdateSectionPositions()
    {
        for (int i = 0; i < sections.Length; i++)
        {
            if (sections[i].transform.position.z - playerCarTransform.position.z < -sectionLength)
            {
                Vector3 lastSectionPosition = sections[i].transform.position;

                sections[i].SetActive(false);

                sections[i] = GetRandomSectionFromPool();

                // MODIFICATION : On conserve les coordonnées X et Y de la route précédente
                float newZ = lastSectionPosition.z + (sectionLength * sections.Length);
                sections[i].transform.position = new Vector3(lastSectionPosition.x, lastSectionPosition.y, newZ);

                sections[i].SetActive(true);
            }
        }
    }

    GameObject GetRandomSectionFromPool()
    {
        int randomIndex = Random.Range(0, sectionsPool.Length);
        bool isNewSectionFound = false;

        while (!isNewSectionFound)
        {
            if (!sectionsPool[randomIndex].activeInHierarchy)
                isNewSectionFound = true;
            else
            {
                randomIndex++;

                if (randomIndex >= sectionsPool.Length)
                    randomIndex = 0;
            }
        }
        return sectionsPool[randomIndex];
    }

    // NOUVEAUTÉ : Dessine un repère visuel dans l'éditeur Unity
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        // Dessine une petite sphère au point de départ exact
        Gizmos.DrawSphere(transform.position, 2f);

        // Dessine 5 boîtes pour prévisualiser l'emplacement et la taille des 5 premières routes
        for (int i = 0; i < 5; i++)
        {
            // On calcule le centre de la boîte visuelle
            Vector3 centerPos = transform.position + new Vector3(0, 0, (i * sectionLength) + (sectionLength / 2f));

            // On dessine une boîte (la largeur de 20f et l'épaisseur de 1f sont juste là pour faire joli dans l'éditeur)
            Gizmos.DrawWireCube(centerPos, new Vector3(20f, 1f, sectionLength));
        }
    }
}