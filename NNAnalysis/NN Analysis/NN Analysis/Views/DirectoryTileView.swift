//
//  DirectoryTileView.swift
//  Neural Analysis
//
//  Created by Brandon Kynoch on 2022/07/14.
//

import SwiftUI

struct DirectoryTileView: View {
    @EnvironmentObject var dataHandler: DataHandler
    
    let targetDir: URL
    let targetDirName: String
    
    init(targetDir: URL) {
        self.targetDir = targetDir
        self.targetDirName = targetDir.lastPathComponent
    }
    
    var body: some View {
        SwipableView(mainView: AnyView(VStack {
            // Main View
            ZStack {
                PanelSolidView(colour: S_COL_BACKGROUND2)
                Text(targetDirName)
                    .modifier(TextModifier(size: 16, weight: .medium))
                    .foregroundColor(S_COL_MAIN)
            }
            
        }),
                     leftSide: nil,
                     rightSide: AnyView(VStack {
            // Swipe left view
            
            ZStack {
                PanelSolidView(colour: S_COL_CANCEL)
                
                Image(systemName: "multiply.square")
                    .foregroundColor(S_COL_BACKGROUND2)
                    .modifier(TextModifier(size: 20, weight: .bold))
            }
            .padding(.leading)
            .onTapGesture {
                dataHandler.closeTrainingFolder(path: targetDir)
            }
        }))
    }
}

//struct DirectoryTileView_Previews: PreviewProvider {
//    static var previews: some View {
//        DirectoryTileView()
//    }
//}
