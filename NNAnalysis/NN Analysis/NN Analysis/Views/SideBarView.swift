//
//  SideBarView.swift
//  Neural Analysis
//
//  Created by Brandon Kynoch on 7/13/22.
//

import SwiftUI

struct SideBarView: View {
    @EnvironmentObject var dataHandler: DataHandler
    
    var body: some View {
        ZStack {
            BackgroundView(viewMode: .dark)
            
            if dataHandler.openTrainingFolders.count == 0 {
                FolderSelectionView()
                    .padding()
            } else {
                VStack {
                    HStack {
                        Text("Open Training Folders")
                            .modifier(TextModifier(size: 25, weight: .bold))
                            .foregroundColor(S_COL_BACKGROUND2)
                        Spacer()
                    }
                        
                    Spacer().frame(height: 20)
                    
                    ScrollView {
                        ForEach (dataHandler.openTrainingFolders, id: \.self) { folder in
                            DirectoryTileView(targetDir: folder.folder)
                                .frame(height: 50)
                                .padding(.leading)
                                .padding(.trailing)
                        }
                    }
                    
                    Spacer()
                    
                    SelectFolderButton()
                    
                    Spacer().frame(height: 50)
                }
                .padding()
            }
        }
    }
}

struct SideBarView_Previews: PreviewProvider {
    static var previews: some View {
        SideBarView()
    }
}
