//
//  FolderSelectionView.swift
//  NN Analysis
//
//  Created by Brandon Kynoch on 7/13/22.
//

import SwiftUI

struct FolderSelectionView: View {
    var body: some View {
        VStack {
            Spacer()
            
            HStack {
                Spacer()
                Text("Neural Network Analysis & Shit")
                    .modifier(TextModifier(size: 30, weight: .bold))
                    .foregroundColor(S_COL_MAIN)
                Spacer()
            }
            
            Spacer().frame(height:40)
            
            SelectFolderButton()
            
            Spacer()
        }
        .padding()
    }
}

struct SelectFolderButton: View {
    var body: some View {
        ZStack {
            PanelSolidView(colour: S_COL_MAIN)
            
            PanelView(type: .behindWindow)
                .mask(
                    Text("Select training folder")
                        .modifier(TextModifier(size: 20, weight: .semibold))
                )
        }
        .frame(width: 500, height: 60)
        .onTapGesture {
            selectFolder(onCompletion: { files in
                withAnimation {
                    for file in files {
                        DataHandler.singleton.openTrainingFolder(fromPath: file)
                    }
                }
            })
        }
    }
    
    func selectFolder(onCompletion: @escaping ([URL]) -> ()) {
        let folderChooserPoint = CGPoint(x: 0, y: 0)
        let folderChooserSize = CGSize(width: 500, height: 600)
        let folderChooserRectangle = CGRect(origin: folderChooserPoint, size: folderChooserSize)
        let folderPicker = NSOpenPanel(contentRect: folderChooserRectangle, styleMask: .utilityWindow, backing: .buffered, defer: true)
        
        folderPicker.canChooseDirectories = true
        folderPicker.canChooseFiles = true
        folderPicker.allowsMultipleSelection = true
        folderPicker.canDownloadUbiquitousContents = true
        folderPicker.canResolveUbiquitousConflicts = true
        
        folderPicker.begin { response in
            if response == .OK {
                onCompletion(folderPicker.urls)
            }
        }
    }
}

struct FolderSelectionView_Previews: PreviewProvider {
    static var previews: some View {
        FolderSelectionView()
    }
}
