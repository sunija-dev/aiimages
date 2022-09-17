/*
 * DEBUG with timestamps
 *
 * For details, visit the Rombos blog:
 *  http://rombosblog.wordpress.com/2014/02/01/unity-debug-log-console-messages-with-timestamp/ 
 *
 * Copyright (c) 2014 Hans-Juergen Richstein, Rombos
 * http://www.rombos.de
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 *
 */

using UnityEngine;
using System.Collections;

public class Debug
{
	// ******** Extended ******** 
	
	static string getTimestamp() {
		return "[" + System.DateTime.UtcNow.ToString() + "] ";
		
		// or try this if you need a date in front:
		//return System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff: ");
	}
	
	public static void Log(object obj, Object context = null) {
		if (context == null) 
			UnityEngine.Debug.Log( getTimestamp() + obj );
		else
			UnityEngine.Debug.Log( getTimestamp() + obj, context );
	}
	
	public static void LogError(object obj, Object context = null) {
		if (context == null) 
			UnityEngine.Debug.LogError( getTimestamp() + obj );
		else
			UnityEngine.Debug.LogError( getTimestamp() + obj , context);
	}
	
	public static void LogWarning(object obj, Object context = null) {
		if (context == null) 
			UnityEngine.Debug.LogWarning( getTimestamp() + obj );
		else
			UnityEngine.Debug.LogWarning( getTimestamp() + obj , context);
	}
	
	
	// ******** Mirrored ******** 
	
	
	public static void LogException(System.Exception exception, Object context = null) {
		// no easy way to inject timestanp here
		UnityEngine.Debug.LogException(exception , context);
	}
	
	public static bool developerConsoleVisible {
		get {return UnityEngine.Debug.developerConsoleVisible;}
		set {UnityEngine.Debug.developerConsoleVisible = value;}
	}
	
	public static bool isDebugBuild {
		get {return UnityEngine.Debug.isDebugBuild;}
	}
	
	public static void DrawLine(Vector3 start, Vector3 end, Color? color = null, float duration = 0.0f, bool depthTest = true) {
		Color col = color.HasValue ? color.Value : Color.white; // workaround for problem with color constant as default value 
		UnityEngine.Debug.DrawLine(start, end, col, duration, depthTest);
	}
	
	public static void DrawRay(Vector3 start, Vector3 dir, Color? color = null, float duration = 0.0f, bool depthTest = true) {
		Color col = color.HasValue ? color.Value : Color.white; // workaround for problem with color constant as default value 
		UnityEngine.Debug.DrawRay(start, dir, col, duration, depthTest);
	}
	
	public static void Break() {
		UnityEngine.Debug.Break();
	}
	
	public static void DebugBreak() {
		UnityEngine.Debug.DebugBreak();
	}
	
	public static void ClearDeveloperConsole() {
		UnityEngine.Debug.ClearDeveloperConsole();
	}
}
