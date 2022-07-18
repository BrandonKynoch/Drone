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
            Rectangle()
                .foregroundColor(COL_BACKGROUND)
            
            if dataHandler.openNetworkEntityFolders.count == 0 {
                FolderSelectionView()
                    .padding()
            } else {
                VStack {
                    HStack {
                        Text("Open Training Folders")
                            .modifier(TitleTextModifier())
                            .foregroundColor(COL_TEXT)
                        Spacer()
                    }
                        
                    Spacer().frame(height: 20)
                    
                    ScrollView {
                        ForEach (dataHandler.epochFolders, id: \.self) { epochFolder in
                            EpochTileView(epoch: epochFolder)
                                .padding()
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
