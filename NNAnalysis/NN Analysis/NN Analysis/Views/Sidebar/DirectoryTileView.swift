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
    var dirString: String
    let targetDirName: String
    
    init(targetDir: URL) {
        self.targetDir = targetDir
        let pathComponents: [String] = targetDir.pathComponents.reversed()
        self.dirString = ""
        for i in 1..<(min(pathComponents.count, 3)) {
            self.dirString = "\(pathComponents[i])/\(self.dirString)"
        }
        self.targetDirName = targetDir.lastPathComponent
    }
    
    var body: some View {
        SwipableView(mainView: AnyView(VStack {
            // Main View
            ZStack {
                PanelSolidView(colour: COL_BACKGROUND2)
                HStack {
                    Text(dirString)
                        .modifier(LightTextModifier())
                        .foregroundColor(COL_TEXT)
                    Spacer().frame(width: ELEMENT_SPACING)
                    Text(targetDirName)
                        .modifier(BodyTextModifier())
                        .foregroundColor(COL_TEXT)
                }
                .padding()
            }
            .onTapGesture {
                dataHandler.setCurrentViewingTrainingFolder(path: targetDir)
            }
        }),
                     leftSide: nil,
                     rightSide: AnyView(VStack {
            // Swipe left view
            
            ZStack {
                PanelSolidView(colour: COL_CANCEL)
                
                Image(systemName: "multiply.square")
                    .foregroundColor(COL_BACKGROUND)
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
