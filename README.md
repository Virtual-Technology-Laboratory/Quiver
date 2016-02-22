# Quiver
A Unity 5.3.2 vector flow overlay visualization generator which utilizes .BYTES files to deliver an arrow visual of vector/magnitude data.

Quiver//REQUIREMENTS:
-Unity5.3.xxx or later
-VTL SimTimeControls
-A ground object, terrain or other surface collider on its own layer

Quiver//SETUP:
-Place the Quiver Prefab in the scene
-Create or use and existing UI Canvas and place the SimTimeControls Prefab within.
-Compare your Quiver object's inspector with the provided sample.
-Ensure the Arrow, FileIndicator, and Vector Labels prefabs are assigned in the Quiver inspector
-Assign the SimTimeControls you just created to the appropriate place in the Quiver object's inspector
-If necessary, create a layer to contain the arrow objects and the ground/terrain object(s) and assign those to their respect places on the Quiver object's inspector
-Place all datafiles to be used in a folder titled "Resources" in the Assets folder of your project. It is very important that all files used are in Assets/Resources, otherwise the cannot be read and will throw an error. Do not include any other subfolders. 
-Assign files from the Resources folder to the Data Files list in the Quiver object's inspector
-Assign values to the Area Dimensions. These describe the area within which the vectors will populate, using the Quiver's transform as the origin. If you need to "flip" the data, reverse the sign on the corresponding axis. Ensure there is ground/terrain underneath any portion within the white box where data might be generated by moving the Quiver object in the scene. Rotation does not affect the generation of vectors.

Quiver//NOTES:
-To speed up playback, increase the SimTimeControls>SpeedControlSlider Max Value to 16

-------------------------------------------
This software is licensed under the BSD 3-Clause License

Copyright (c) 2015, Jacob Cooper (coop3683@outlook.com)
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, 
are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this 
   list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice, 
   this list of conditions and the following disclaimer in the documentation 
   and/or other materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its contributors 
   may be used to endorse or promote products derived from this software without 
   specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE 
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR 
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON 
ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (
INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS 
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
