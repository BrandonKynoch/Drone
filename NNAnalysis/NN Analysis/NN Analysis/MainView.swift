//
//  ContentView.swift
//  NN Analysis
//
//  Created by Brandon Kynoch on 2022/07/10.
//

import SwiftUI

let COL_TEXT: Color = .white
let COL_ACC0: Color = Color(nsColor: NSColor(hex: "#F20544")!)
let COL_ACC1: Color = Color(nsColor: NSColor(hex: "#D90467")!)
let COL_ACC2: Color = Color(nsColor: NSColor(hex: "#F26200")!)
let COL_ACC3: Color = Color(nsColor: NSColor(hex: "#F2A516")!)
let COL_BACKGROUND: Color = Color(nsColor: NSColor(hex: "#26000A")!)
let COL_BACKGROUND2: Color = Color(nsColor: NSColor(hex: "#750020")!)

let COL_CANCEL: Color = Color(nsColor: NSColor(hex: "#DE1D12")!)
let COL_APPROVE: Color = Color(nsColor: NSColor(hex: "#49DE1D")!)

let SHADOW_COL: Color = Color.black.opacity(0.5)

let CORNER_RADIUS: CGFloat = 12

let ELEMENT_SPACING: CGFloat = 10

enum ViewMode {
    case light
    case dark
}

struct MainView: View {
    @EnvironmentObject var dataHandler: DataHandler
    
    var body: some View {
        NavigationView {
            SideBarView()
            .toolbar {
                ToolbarItem(placement: .navigation) {
                    Button(action: toggleSidebar, label: { // 1
                        Image(systemName: "sidebar.leading")
                    })
                }
            }
            
            if dataHandler.currentViewingTrainingFolder?.nng != nil {
//                NNGroupView(nnGroup: dataHandler.currentViewingTrainingFolder!.nng!)
                NNGroupView(nnGroupFolder: dataHandler.currentViewingTrainingFolder!)
            } else {
                EmptyMainView()
            }
            
            // TODO: Improve method of loading epochs so that it will not only load single drone if name is 0
        }
        .background(KeyEventHandling())
    }
    
    private func toggleSidebar() { // 2
        #if os(iOS)
        #else
        NSApp.keyWindow?.firstResponder?.tryToPerform(#selector(NSSplitViewController.toggleSidebar(_:)), with: nil)
        #endif
    }
}

struct MainView_Previews: PreviewProvider {
    static var previews: some View {
        MainView()
    }
}
