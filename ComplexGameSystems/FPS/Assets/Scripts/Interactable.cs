﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    string GetTitle();
    void Pickup();
    void Drop();
}