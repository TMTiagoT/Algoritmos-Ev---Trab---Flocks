using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CompositeBehavior))] //definir como custom editor
public class CompositeBehaviorEditor : Editor
{
    //public


    //private
    private int auxRemovePosition = 0;

    public override void OnInspectorGUI() //sobrescrever a funcao de desenhar no inspector da classe especificada
    {
        CompositeBehavior compositeBehavior = (CompositeBehavior)target; //inicializar valores

        DrawDefaultInspector(); //desenhar o inspector default

        if (compositeBehavior.flockBehaviors == null || compositeBehavior.flockBehaviors.Length == 0) //se nao existir ou nao tiverem comportamentos no array flockBehaviors
        {
            EditorGUILayout.BeginHorizontal(); //iniciar horizontal

            EditorGUILayout.HelpBox("No Behaviors in Array!", MessageType.Warning); //marcar um aviso

            EditorGUILayout.EndHorizontal(); //resetar horizontal
        }
        else //caso contrario
        {
            EditorGUILayout.BeginHorizontal(); //iniciar horizontal

            GUILayout.Space(18);
            EditorGUILayout.LabelField(new GUIContent("Behaviors"), EditorStyles.boldLabel, GUILayout.MaxWidth(75));
            GUILayout.FlexibleSpace(); //espaco auto reajustavel
            EditorGUILayout.LabelField(new GUIContent("Weights"), EditorStyles.boldLabel, GUILayout.MaxWidth(60));

            EditorGUILayout.EndHorizontal(); //finalizar horizontal

            for (int i = 0; i < compositeBehavior.flockBehaviors.Length; i++) //para cada comportamento no array
            {
                EditorGUILayout.BeginHorizontal(); //inciar horizontal

                EditorGUILayout.LabelField(new GUIContent("" + i), GUILayout.MaxWidth(12));
                compositeBehavior.flockBehaviors[i] = (FlockBehavior)EditorGUILayout.ObjectField(compositeBehavior.flockBehaviors[i], typeof(FlockBehavior), false);
                GUILayout.Space(3);
                compositeBehavior.behaviorsWeights[i] = EditorGUILayout.FloatField(compositeBehavior.behaviorsWeights[i]);

                EditorGUILayout.EndHorizontal(); //finalizar horizontal
            }
        }

        EditorGUILayout.Space(); //espaco
        EditorGUILayout.BeginHorizontal(); //inciar horizontal

        if (GUILayout.Button(new GUIContent("Add Behavior"))) //caso pressionar o botao, adicionar um comportamento
        {
            GUI_AddBehavior(compositeBehavior);
        }

        EditorGUI.BeginDisabledGroup(compositeBehavior.flockBehaviors == null || compositeBehavior.flockBehaviors.Length == 0); //se nao existir ou nao tiverem comportamentos no array flockBehaviors //inciar grupo "desativado"
        if (GUILayout.Button(new GUIContent("Remove Behavior"))) //caso pressionar o botao, remover um comportamento
        {
            GUI_RemoveBehavior(compositeBehavior, auxRemovePosition);
        }

        EditorGUILayout.EndHorizontal(); //finalizar horizontal

        EditorGUILayout.BeginHorizontal(); //inciar horizontal

        GUILayout.FlexibleSpace(); //espaco auto reajustavel
        auxRemovePosition = EditorGUILayout.IntField(auxRemovePosition, GUILayout.MaxWidth(100));
        EditorGUI.EndDisabledGroup(); //finalizar grupo "desativado"

        EditorGUILayout.EndHorizontal(); //finalizar horizontal
    }

    public void GUI_AddBehavior(CompositeBehavior compositeBehavior) //adicionar um comportamento
    {
        int newArraySize = 0; //incializar valores

        if (compositeBehavior.flockBehaviors == null) //se nao existir o array, criar
        {
            compositeBehavior.flockBehaviors = new FlockBehavior[0];
            compositeBehavior.behaviorsWeights = new float[0];
        }

        newArraySize = compositeBehavior.flockBehaviors.Length + 1; //pegar tamanho + 1

        FlockBehavior[] newFlockBehaviors = new FlockBehavior[newArraySize]; //incializar valores
        float[] newBehaviorsWeights = new float[newArraySize];

        for (int i = 0; i < newArraySize - 1; i++) //para cada elemento antigo, re adicionar no novo array
        {
            newFlockBehaviors[i] = compositeBehavior.flockBehaviors[i];
            newBehaviorsWeights[i] = compositeBehavior.behaviorsWeights[i];
        }

        newBehaviorsWeights[newArraySize - 1] = 1f; //adicionar valor default ao novo elemento

        compositeBehavior.flockBehaviors = newFlockBehaviors; //setar novos arrays no lguar dos antigos
        compositeBehavior.behaviorsWeights = newBehaviorsWeights;
    }

    public void GUI_RemoveBehavior(CompositeBehavior compositeBehavior, int position) //remover um comportamento
    {
        position = Mathf.Clamp(position, 0, compositeBehavior.flockBehaviors.Length); //ajustar posicao para nao passar o array

        int newArraySize = 0; //incializar valores

        if (compositeBehavior.flockBehaviors != null) //se existir o array, pegar tamanho + 1
        {
            newArraySize = compositeBehavior.flockBehaviors.Length - 1;
        }

        FlockBehavior[] newFlockBehaviors = new FlockBehavior[newArraySize]; //incializar valores
        float[] newBehaviorsWeights = new float[newArraySize];

        for (int i = 0; i < newArraySize; i++) //para cada elemento antigo, re adicionar no novo array
        {
            newFlockBehaviors[i] = compositeBehavior.flockBehaviors[i >= position ? i + 1 : i];
            newBehaviorsWeights[i] = compositeBehavior.behaviorsWeights[i >= position ? i + 1 : i];
        }

        compositeBehavior.flockBehaviors = newArraySize == 0 ? null : newFlockBehaviors; //setar novos arrays no lguar dos antigos
        compositeBehavior.behaviorsWeights = newArraySize == 0 ? null : newBehaviorsWeights;
    }
}
