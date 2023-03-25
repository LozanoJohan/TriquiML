using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using TMPro;

public class MLAgent : Agent
{
    public int x; //El juegador juagara con x (0) o con o (1)
    public MeshRenderer mr;
    public Material material;
    public Material orange;
    public Transform[] cuadrantes;
    public GameObject clickedObj;
    public Transform[,] tablero =  new Transform[3, 3];
    int agentesListos = 0;
    public GameObject other;

    public float time = 2f;

    bool act = false;
    
    void Awake()
    {
        InicializarTablero();
    }

    void SetDecisionRequester()
    {
        other.GetComponent<MLAgent>().act = true;
    }

    public override void OnEpisodeBegin()
    {
        QualitySettings.vSyncCount = 4;  //Controllador de fps, no vaya a ser que se te explote la pc ekisde
        Application.targetFrameRate= -1;
        
        RestartTablero();
        if (x == 0) Invoke("SetDecisionRequester", time);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        foreach (var cuadrante in cuadrantes)
        {
            int cuadranteState;
        
            if (!cuadrante.GetChild(0).gameObject.activeSelf && !cuadrante.GetChild(1).gameObject.activeSelf) cuadranteState = 0;
            else if (cuadrante.GetChild(0).gameObject.activeSelf) cuadranteState = x == 0 ? 1 : 2;
            else cuadranteState = x == 0 ? 2 : 1;

            sensor.AddObservation(cuadranteState);
        }
    }
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        if (act)
        {
        //Debug.Log("a");
            int cuadrante = actions.DiscreteActions[0];

            if (cuadrantes[cuadrante].GetChild(0).gameObject.activeSelf 
                    || cuadrantes[cuadrante].GetChild(1).gameObject.activeSelf){
                AddReward(-2);
               //PutItem(cuadrante, x);
            } else
            {
                PutItem(cuadrante, x);
                if (VerificarGanador(x))
                {
                    AddReward(1f);
                    Invoke("EndEpisode", 1f);
                    other.GetComponent<Agent>().AddReward(-1);

                    mr.material = material;
                }

                Invoke("SetDecisionRequester", time);
                act = false;
            }
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        //discreteActions[0] = int.Parse(clickedObj.name);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)){ 
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit rh;
        if (Physics.Raycast(ray, out rh)){ 
            if (rh.collider.gameObject.CompareTag("Cuad")){
                    clickedObj = rh.collider.gameObject;
                    //Debug.Log(clickedObj);
                }
            }
        }
    }

    public void RestartTablero()
    {
        foreach (Transform cuadrante in cuadrantes)
        {
            foreach (Transform item in cuadrante) //por cada X o O que halla en el tablero...
            {
                item.gameObject.SetActive(false);
            }
        }
        mr.material = orange;
    }

    public void PutItem(int cuadrante, int x)
    {
        cuadrantes[cuadrante].GetChild(x).gameObject.SetActive(true);
    }

    bool VerificarGanador(int x) {
        // Verificar si hay alguna fila completa
        for (int fila = 0; fila < 3; fila++) {

            if (tablero[fila, 0].GetChild(x).gameObject.activeSelf && tablero[fila, 1].GetChild(x).gameObject.activeSelf && tablero[fila, 2].GetChild(x).gameObject.activeSelf)  {
                return true;
            }
            
        }
        // Verificar si hay alguna columna completa

        for (int columna = 0; columna < 3; columna++) {
            //Debug.Log(tablero[2, columna].GetChild(x));

            if (tablero[0, columna].GetChild(x).gameObject.activeSelf && tablero[1, columna].GetChild(x).gameObject.activeSelf && tablero[2, columna].GetChild(x).gameObject.activeSelf)  {
                return true;
            }
        }

        // Verificar si hay alguna diagonal completa
        if (tablero[0, 0].GetChild(x).gameObject.activeSelf && tablero[1, 1].GetChild(x).gameObject.activeSelf && tablero[2, 2].GetChild(x).gameObject.activeSelf)  {
            return true;
        }
        if (tablero[0, 2].GetChild(x).gameObject.activeSelf && tablero[1, 1].GetChild(x).gameObject.activeSelf && tablero[2, 0].GetChild(x).gameObject.activeSelf)  {
            return true;
        }

         int cuadrosLenos = 0;
        foreach (Transform cuadrante in cuadrantes)
        {
            foreach (Transform item in cuadrante) //por cada X o O que halla en el tablero...
            {
                if (item.gameObject.activeSelf) 
                        cuadrosLenos++;
            }
        }
        if (cuadrosLenos == 9) 
        {
            EndEpisode();
            AddReward(-1);
        }

            // Si llegamos aquí, no hay ganador
        return false;
    }

        

    void InicializarTablero() {
        int i = 0;
        // Llenar todas las casillas con espacio en blanco
        for (int fila = 0; fila < 3; fila++) {
            for (int columna = 0; columna < 3; columna++) {
                tablero[fila, columna] = cuadrantes[i];

                i++;
            }
        }
    }
}
