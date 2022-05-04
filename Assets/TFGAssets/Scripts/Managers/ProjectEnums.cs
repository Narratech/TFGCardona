using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Gesture Manager
public enum eHandUsage
{
    NOHAND,
    LEFT_HAND_ONLY,
    RIGHT_HAND_ONLY,
    BOTH_HANDS
};
public enum eGesturePhase
{
    GESTURE_SIMPLE,   // Gestos sin movimiento
    GESTURE_BEGIN,    // Inicio de gesto en movimiento
    GESTURE_END,       // Fin gesto en movimiento
    NONE
}
public enum eGestureCategory
{
    GESTURE_WORD,           // Si es una palabra (añadir espacio despues de ella)
    GESTURE_LETTER,         // Si es una letra (no añadir espacio)
    GESTURE_LETTER_OR_WORD, // Cuando tiene ambas componentes
    GESTURE_COMMAND,
    NONE
}
public enum eESLalphabet
{
    A,
    B,
    C,
    CH,
    D,
    E,
    F,
    G,
    H,
    I,
    J,
    K,
    L,
    LL,
    M,
    N,
    Ñ,
    O,
    P,
    Q,
    R,
    RR,
    S,
    T,
    U,
    V,
    W,
    X,
    Y,
    Z
};

// Capture Button
enum eButtonMode
{
    GESTURE_CAPTURE,
    HAND_CONF,
    PHASE_CONF,
    SEND_COMMAND
}

// Text manager
public enum eMessageSource
{
    DEBUG,
    VOICE,
    HAND_SIGN
}

// Button Behavior
public enum eKeyCommand
{
    Q,
    W,
    E,
    R,
    T,
    Y,
    U,
    I,
    O,
    P,
    A,
    S,
    D,
    F,
    G,
    H,
    J,
    K,
    L,
    Ñ,
    Z,
    X,
    C,
    V,
    B,
    N,
    M,
    ENTER,
    BACKSPACE,
    BACK,
    CLEAR
}

enum eConnectionType
{
    ANONYMOUS,
    WITHNAME,
    CONNECT
}

// Button Join Room
enum eMapType
{
    Exteriores,
    Aula,
    Random
}

public enum eScenes
{
    DEVELOPMENT,
    LOGIN,
    ROOM_SELECTION,
    OUTDOORS,
    CLASSROOM,
    CAPTURE,
    TUTORIAL
}

public class ProjectEnums : MonoBehaviour
{
}
