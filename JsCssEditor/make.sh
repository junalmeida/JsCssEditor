#!/bin/bash

mdtool=/Applications/MonoDevelop.app/Contents/MacOS/mdtool

cd bin/Release

$mdtool setup pack ./JsCssEditor.addin.xml -d:../../../Release
$mdtool setup rep-build ../../../Release

cd ../..